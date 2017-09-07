using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Search;
using Ayane.Common;
using Ayane.FrameworkEx;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.WinRT;

namespace Ayane.Models
{

    #region Main

    partial class Playlist : IDisposable
    {
        public string Title { get; set; }
        public ObservableCollection<Song> Songs { get; private set; } = new ObservableCollection<Song>();
        private IList<PlaylistFolder> Folders { get; set; } = new List<PlaylistFolder>();
        public Uri CoverUri => Songs.FirstOrDefault(s => s.CoverUri != null)?.CoverUri;
        public bool Initialized { get; private set; }
        public bool HasSyncFolders => Folders.Count > 0;

        private const string PlaylistFolder = "Playlist";
        private SQLiteConnection _db;

        public Playlist(string title)
        {
            if (!Playlist.IsTitleLegal(title)) throw new ArgumentException("Illegal playlist name");
            Title = title;

            var folder = ApplicationData.Current.LocalFolder.CreateFolderAsync(PlaylistFolder, CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
            _db = new SQLiteConnection(new SQLitePlatformWinRT(), Path.Combine(folder.Path, $"{Title}.db"));
            _db.CreateTable<Song>();
            _db.CreateTable<PlaylistFolder>();
        }

        public IReadOnlyList<Album> Albums { get; private set; }

        public async Task<IReadOnlyList<Album>> RefreshAlbumsAsync()
        {
            Albums = await Task.Run(() => Songs.GroupBy(s => s.AlbumTitle).Select(g =>
            {
                var album = new Album { Title = g.Key, Songs = g.OrderBy(s => s.TrackNumber).ToList(), Year = g.FirstOrDefault()?.Year ?? 0 };

                for (int i = 0; i < album.Songs.Count; i++)
                {
                    var song = album.Songs[i];
                    song.Album = album;
                    song.TrackNumber = (uint)(i + 1);
                }

                return album;
            }).AsParallel().ToList());

            return Albums;
        }

        public async void RefreshAlbums() => await RefreshAlbumsAsync();

        public IReadOnlyList<Artist> Artists { get; private set; }

        public async Task<IReadOnlyList<Artist>> RefreshArtistsAsync()
        {
            await RefreshAlbumsAsync();

            var albumComparer = new Album.AlbumEqualityComparer();
            Artists = await Task.Run(() => Songs.GroupBy(s => s.ArtistName).Select(g =>
            {
                var artist = new Artist { Name = g.Key, };

                foreach (var result in g.Select(s => s.Album))
                {
                    result.Artist = artist;
                }

                artist.Albums = g.Select(item => item.Album).Distinct(albumComparer).ToList();
                artist.SongsCount = artist.Albums?.Sum(a => a.Songs?.Count ?? 0) ?? 0;
                return artist;
            }).AsParallel().ToList());

            return Artists;
        }

        public async void RefreshArtists() => await RefreshArtistsAsync();

        public IReadOnlyList<Genre> Genres { get; private set; }

        public async Task<IReadOnlyList<Genre>> RefreshGenresAsync()
        {
            Genres = await Task.Run(() => Songs.GroupBy(s => s.GenreTitle.ToUpper()).Select(g => new Genre { Title = g.FirstOrDefault().GenreTitle ?? g.Key, Songs = g.ToList() }).AsParallel().ToList());
            return Genres;
        }

        public async void RefreshGenres() => await RefreshGenresAsync();

        public void Dispose()
        {
            foreach (var song in Songs)
            {
                if (song.Artist?.Albums != null)
                {
                    foreach (var artistAlbum in song.Artist.Albums)
                    {
                        artistAlbum.Songs?.Clear();
                        artistAlbum.Artist = null;
                    }
                }

                song.Artist?.Albums?.Clear();
                song.Album = null;
                song.Artist = null;
            }

            Songs?.Clear();
            _db?.Dispose();
            _db = null;
            Initialized = false;
        }

        public async Task InitializeAsync()
        {
            if (Initialized) return;

            Songs = await Task.Run(() => new ObservableCollection<Song>(_db.Table<Song>().OrderBy(s => s.Title).ThenBy(s => s.ArtistName).ThenBy(s => s.AlbumTitle).ThenBy(s => s.GenreTitle).AsParallel()));
            Folders = await Task.Run(() => _db.Table<PlaylistFolder>().ToList());
            Initialized = true;
        }

        public async Task<bool> DeleteAsync()
        {
            foreach (var song in Songs)
            {
                song.RemoveToken();
            }

            foreach (var folder in Folders)
            {
                TokensManager.Instance.RemoveToken(folder.Path);
            }

            Dispose();

            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(PlaylistFolder, CreationCollisionOption.OpenIfExists);
                var dbFile = await folder.GetFileAsync($"{Title}.db");
                await dbFile.DeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    #endregion

    [Table("Folder")]
    class PlaylistFolder
    {
        [Column(nameof(Id))]
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [Column(nameof(Token))]
        public string Token { get; set; }

        [Column(nameof(Path))]
        public string Path { get; set; }
    }

    #region Public Methods

    partial class Playlist
    {
        public event EventHandler NewItemImported;

        public void ClearEventListeners()
        {
            NewItemImported = null;
        }

        private async Task ImportFolderAsync(StorageFolder folder)
        {
            var subfolders = await folder.GetFoldersAsync();

            foreach (var subfolder in subfolders)
            {
                await ImportFolderAsync(subfolder);
            }

            var query = folder.CreateFileQueryWithOptions(new QueryOptions(CommonFileQuery.DefaultQuery, SupportedContainers));
            var files = await query.GetFilesAsync();

            if (files.Count == 0) return;
            var folderToken = folder.IsMusicFolder() ? null : Folders.FirstOrDefault(f => f.Path.Equals(folder.Path))?.Token ?? TokensManager.Instance.RequestToken(folder);

            foreach (var audioFile in files)
            {
                if (!await ImportAudioFileAsync(audioFile, folderToken)) continue;
                TokensManager.Instance.IncreaseReferenceCount(folderToken);
            }

            if (Folders.Any(f => f.Path.Equals(folder.Path, StringComparison.OrdinalIgnoreCase))) return;
            var folderRecord = new PlaylistFolder { Path = folder.Path, Token = folderToken };
            Folders.Add(folderRecord);
            _db.Insert(folderRecord);
        }

        private async Task<bool> ImportAudioFileAsync(StorageFile file, string token = null)
        {
            if (file == null) return false;
            if (Songs.Any(s => s.FileUriPath.Equals(file.Path, StringComparison.OrdinalIgnoreCase))) return false;

            token = token ?? (file.IsInMusicFolder() ? null : TokensManager.Instance.RequestToken(file));

            var song = await Task.Run(() => CreateSong(file, token));
            if (song == null) return false;

            _db.Insert(song);
            Songs.Add(song);

            OnNewItemImported();
            return true;
        }

        public async Task ImportStorageItemsAsync(IReadOnlyList<IStorageItem> items)
        {
            foreach (var folder in items.OfType<StorageFolder>())
            {
                await ImportFolderAsync(folder);
            }

            foreach (var file in items.OfType<StorageFile>().Where(f => SupportedContainers.Contains(f.FileType)))
            {
                await ImportAudioFileAsync(file);
            }
        }

        public void RemoveSong(Song song)
        {
            Songs.Remove(song);
            _db.Delete(song);
            song.RemoveToken();
        }

        public async Task Sync()
        {
            var folders = await Task.Run(async () =>
            {
                var dbFolders = new List<StorageFolder>();
                foreach (var item in Folders.Where(f => string.IsNullOrEmpty(f.Token)))
                {
                    try
                    {
                        dbFolders.Add(await StorageFolder.GetFolderFromPathAsync(item.Path));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                foreach (var item in Folders.Where(f => !string.IsNullOrEmpty(f.Token)))
                {
                    var folder = (await TokensManager.Instance.GetStorageItemAsync(item.Token)) as StorageFolder;
                    if (folder == null) continue;
                    dbFolders.Add(folder);
                }

                return dbFolders;
            });

            await ImportStorageItemsAsync(folders);

            var allFiles = await Task.Run(() => folders.Select(f => f.GetAllSubFoldersFileAsync().GetAwaiter().GetResult()).SelectMany(l => l).Select(f => f.Path).ToList());
            var unavailableFiles = Songs.Select(s => s.FileUriPath).Except(allFiles).ToList();
            foreach (var filePath in unavailableFiles)
            {
                var target = Songs.FirstOrDefault(s => s.FileUriPath.Equals(filePath));
                if (target == null) continue;
                RemoveSong(target);
            }
        }

        public async Task<IList<Song>> ReorderSongsAsync()
        {
            return await ReorderSongsAsync((s) => s.Title);
        }

        public async Task<IList<Song>> ReorderSongsAsync<TKey>(Func<Song, TKey> orderFunc)
        {
            Songs = await Task.Run(() => new ObservableCollection<Song>(Songs.OrderBy(orderFunc).ThenBy(s => s.ArtistName).ThenBy(s => s.AlbumTitle).ThenBy(s => s.GenreTitle).AsParallel()));
            return Songs;
        }

        protected virtual void OnNewItemImported()
        {
            NewItemImported?.Invoke(this, EventArgs.Empty);
        }

    }

    #endregion

    #region Misc

    partial class Playlist
    {
        public static async Task<IReadOnlyList<string>> GetPlaylistsTitleAsync()
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(PlaylistFolder, CreationCollisionOption.OpenIfExists);
            var files = await folder.CreateFileQueryWithOptions(new QueryOptions(CommonFileQuery.DefaultQuery, new[] { ".db" })).GetFilesAsync();
            return files.OrderBy(f => f.DateCreated).Select(f => f.DisplayName).ToList();
        }

        public static async Task<bool> IsTitleAvailable(string title)
        {
            if (!IsTitleLegal(title)) return false;

            var titles = await GetPlaylistsTitleAsync();
            return !titles.Select(t => t.ToLower()).Contains(title.ToLower());
        }

        public static bool IsTitleLegal(string title)
        {
            return !string.IsNullOrEmpty(title) && !IllegalFilenameChars.Any(title.Contains) && !IllegalFilenames.Any(n => n.Equals(title, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(title);
        }

        public static bool IsMediaFile(IStorageFile file)
        {
            return file != null && SupportedContainers.Contains(file.FileType);
        }

        public static async Task<bool> DeleteAsync(string title)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(PlaylistFolder, CreationCollisionOption.OpenIfExists);
            try
            {
                var file = await folder.GetFileAsync($"{title}.db");
                await file.DeleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static readonly IReadOnlyList<string> IllegalFilenameChars = new[] { "<", ">", ":", "\"", "/", "\\", "|", "?", "*", ".\\", "..", }; // Contains
        public static readonly IReadOnlyList<string> IllegalFilenames = new[] { "0", "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" }; // Equals
        public static readonly IReadOnlyList<string> SupportedContainers = new[] { ".mp3", ".flac", ".m4a", ".wma", ".mp4", ".aac", ".alac", ".ac3", ".wav" };

        private static Song CreateSong(StorageFile file, string token)
        {
            try
            {
                return new Song(file, token);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }

    #endregion
}

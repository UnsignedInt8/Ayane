using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Ayane.Common;
using Ayane.FrameworkEx;
using Ayane.Models;
using Ayane.Widgets;
using GalaSoft.MvvmLight;

namespace Ayane.ViewModels
{
    partial class PlaylistViewModel : ViewModelBase, IDisposable
    {
        private Playlist _playlist;
        public IList<Song> FlatSongs { get; private set; }
        private IList<AlphaKeyGroup<Song>> _songs;
        private bool _isArtistsRefreshing;
        private ObservableCollection<AlphaKeyGroup<Artist>> _artists;
        private ObservableCollection<KeyGroup<uint, Album>> _albums;
        private ObservableCollection<AlphaKeyGroup<Genre>> _genres;
        private PlayerViewModel PlayerViewModel => ViewModelLocator.Instance.PlayerViewModel;

        public PlaylistViewModel(string title)
        {
            _playlist = new Playlist(title);
            _playlist.InitializeAsync().GetAwaiter().GetResult();
            Songs = AlphaKeyGroup<Song>.CreateGroups(_playlist.Songs, s => s.Title);

            RemoveSongCommand = new ActionCommand<Song>(ExecuteRemoveSong);
            OpenInFileExplorerCommand = new ActionCommand<Song>(ExecuteOpenInFileExplorer);
        }

        public bool HasSongs => Songs.Any(g => g.Any());

        public string Title
        {
            get { return _playlist.Title; }
            set
            {
                _playlist.Title = value;
                RaisePropertyChanged();
            }
        }

        public IList<AlphaKeyGroup<Song>> Songs
        {
            get { return _songs; }
            private set
            {
                _songs = value;
                RaisePropertyChanged();
            }
        }

        public bool IsArtistsRefreshing
        {
            get { return _isArtistsRefreshing; }
            set
            {
                _isArtistsRefreshing = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<AlphaKeyGroup<Artist>> Artists
        {
            get { return _artists; }
            set
            {
                if (_artists == value) return;
                _artists = value;
                RaisePropertyChanged();
            }
        }

        public async Task RefreshArtistsAsync()
        {
            await _playlist.RefreshArtistsAsync();
            Artists = await Task.Run(() => AlphaKeyGroup<Artist>.CreateGroups(_playlist.Artists, a => a.Name));
        }

        public ObservableCollection<KeyGroup<uint, Album>> Albums
        {
            get { return _albums; }
            set
            {
                if (_albums == value) return;
                _albums = value;
                RaisePropertyChanged();
            }
        }

        public async Task RefreshAlbumsAsync()
        {
            await _playlist.RefreshAlbumsAsync();
            Albums = await Task.Run(() => new ObservableCollection<KeyGroup<uint, Album>>(_playlist.Albums.GroupBy(a => a.Year).OrderByDescending(g => g.Key).Select(g => new KeyGroup<uint, Album>(g) { Key = g.Key })));
        }

        public ObservableCollection<AlphaKeyGroup<Genre>> Genres
        {
            get { return _genres; }
            set
            {
                if (_genres == value) return;
                _genres = value;
                RaisePropertyChanged();
            }
        }

        public async Task RefreshGenresAsync()
        {
            await _playlist.RefreshGenresAsync();
            Genres = await Task.Run(() => AlphaKeyGroup<Genre>.CreateGroups(_playlist.Genres, g => g.Title));
        }

        public async void DeletePlaylist()
        {
            await _playlist.DeleteAsync();
            if (PlayerViewModel.PlaylistTitle != Title) return;

            ResetPlayerViewModel();
        }

        public ICommand RemoveSongCommand { get; private set; }
        public ICommand OpenInFileExplorerCommand { get; private set; }

        public void Dispose()
        {
            _playlist.Dispose();
            _playlist = null;
            _artists = null;
        }

        public override string ToString()
        {
            return Title;
        }

        public async Task Sync()
        {
            if (!_playlist.HasSyncFolders)
            {
                Toast.ShowMessage(App.ResourceLoader.GetString("Message_NoFoldersToSync"));
                return;
            }

            var msgBox = new MessageBox
            {
                Title = App.ResourceLoader.GetString("MessageBox_Title_SyncPlaylist"),
                Message = App.ResourceLoader.GetString("MessageBox_Message_SyncPlaylist"),
                ButtonsVisibility = Visibility.Collapsed,
            };
            msgBox.Show();

            FlatSongs = null;
            var songsCount = _playlist.Songs.Count;
            await _playlist.Sync();

            var newGroups = AlphaKeyGroup<Song>.CreateGroups(_playlist.Songs, s => s.Title);
            foreach (var group in Songs)
            {
                group.Clear();
                var newGroup = newGroups.FirstOrDefault(g => g.Key == group.Key);
                if (newGroup == null || newGroup.Count == 0) continue;

                foreach (var item in newGroup)
                {
                    group.Add(item);
                }
            }

            if (PlayerViewModel.PlaylistTitle == Title && songsCount != _playlist.Songs.Count)
            {
                ResetPlayerViewModel();
            }

            msgBox.Close();
        }

        private void ResetPlayerViewModel()
        {
            PlayerViewModel.Stop();
            PlayerViewModel.ClearQueue();
            PlayerViewModel.ClearActiveSong();
        }
    }

    /// <summary>
    /// Commands Area
    /// </summary>
    partial class PlaylistViewModel
    {
        private void ExecuteRemoveSong(Song song)
        {
            if (song == null) return;

            var msgBox = new MessageBox
            {
                Title = App.ResourceLoader.GetString("MessageBox_Title_RemoveSong"),
                Message = App.ResourceLoader.GetString("MessageBox_Message_RemoveSong"),
                PositiveButtonTitle = App.ResourceLoader.GetString("MessageBox_Title_PositiveButton"),
                NegativeButtonTitle = App.ResourceLoader.GetString("MessageBox_Title_NegativeButton"),
            };

            msgBox.PositiveButtonClick += (sender, args) => RemoveSong(song);
            msgBox.Show();
        }

        private void RemoveSong(Song song)
        {
            FlatSongs = null;
            _playlist.RemoveSong(song);

            foreach (var group in Songs.Where(g => g.Contains(song)))
            {
                group.Remove(song);
            }

            RaisePropertyChanged(nameof(HasSongs));

            if (PlayerViewModel.PlaylistTitle != Title) return;
            PlayerViewModel.RemoveQueueSong(song);
        }

        private async void ExecuteOpenInFileExplorer(Song obj)
        {
            if (obj == null) return;

            try
            {
                var file = await obj.ToStorageFileAsync();
                var option = new FolderLauncherOptions();
                if (file != null) option.ItemsToSelect.Add(file);
                await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(file?.Path ?? obj.FileUriPath)), option);
            }
            catch (Exception)
            {

            }

        }
    }

    /// <summary>
    /// UI events
    /// </summary>
    partial class PlaylistViewModel
    {
        public void OnDragOver(object sender, DragEventArgs e)
        {
            if (MediaLibraryViewModel.CheckTokensThresold())
            {
                e.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            if (e.DataView.Contains(StandardDataFormats.StorageItems)) e.AcceptedOperation = DataPackageOperation.Copy;
        }

        public async void OnDrop(object sender, DragEventArgs e)
        {
            try
            {
                await OnDropAsync(sender, e);
            }
            catch (Exception ex)
            {
                Toast.ShowMessage(ex.Message);
            }

            RaisePropertyChanged(nameof(HasSongs));
        }

        private async Task OnDropAsync(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            FlatSongs = null;

            var processedText = App.ResourceLoader.GetString("MessageBox_Message_ProcessedFiles");
            var msgBox = new MessageBox
            {
                Title = App.ResourceLoader.GetString("MessageBox_Title_JustAMinute"),
                Message = $"{processedText} 0",
                ButtonsVisibility = Visibility.Collapsed,
            };

            msgBox.Show();

            var existsCount = _playlist.Songs.Count;
            var items = await e.DataView.GetStorageItemsAsync();
            _playlist.NewItemImported += (o, args) => msgBox.Message = $"{processedText} {_playlist.Songs.Count - existsCount}";

            await _playlist.ImportStorageItemsAsync(items);
            _playlist.ClearEventListeners();

            msgBox.Message = App.ResourceLoader.GetString("MessageBox_Message_ListUpdating");

            // Insert item by text order
            var groups = await Task.Run(() => AlphaKeyGroup<Song>.CreateGroups(_playlist.Songs, s => s.Title));
            foreach (var songGroup in Songs)
            {
                var newGroup = groups.FirstOrDefault(g => g.Key == songGroup.Key);
                if (newGroup == null) continue;

                foreach (var song in newGroup)
                {
                    if (songGroup.Contains(song)) continue;
                    var behindSong = songGroup.FirstOrDefault(item => string.CompareOrdinal(item.Title, song.Title) > 0);
                    if (behindSong != null)
                    {
                        var index = songGroup.IndexOf(behindSong);
                        songGroup.Insert(Math.Max(0, Math.Min(index, songGroup.Count - 1)), song);
                        continue;
                    }

                    songGroup.Add(song);
                }
            }

            var newGroups = groups.Except(Songs, new AlphaKeyGroup<Song>.KeyComparer<Song>());
            foreach (var newGroup in newGroups)
            {
                var behindGroup = Songs.FirstOrDefault(uiGroup => string.CompareOrdinal(uiGroup.Key, newGroup.Key) > 0);
                if (behindGroup != null)
                {
                    var behindIndex = Songs.IndexOf(behindGroup);
                    Songs.Insert(Math.Max(0, Math.Min(behindIndex, Songs.Count - 1)), newGroup);
                    continue;
                }

                Songs.Add(newGroup);
            }

            msgBox.Close();
        }

        public void SongsOnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch) return;
            PlaySongs(sender as ListViewBase, false);
        }

        public void SongsOnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Touch) return;
            PlaySongs(sender as ListViewBase, true);
        }

        private void PlaySongs(ListViewBase sender, bool force)
        {
            if (sender == null) return;
            PlayerViewModel.PlaylistTitle = Title;

            if (FlatSongs == null) FlatSongs = GetFlatSongs();
            PlayerViewModel.AutoPlay = true;
            PlayerViewModel.Play(FlatSongs, sender.SelectedIndex, force);
        }

        public IList<Song> GetFlatSongs()
        {
            if (Songs == null || Songs.Count == 0) return new List<Song>();
            return Songs.SelectMany(g => g).AsParallel().ToList();
        }

        public void ArtistsOnItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as Artist;
            if (artist == null) return;

            ViewModelLocator.Instance.SecondaryPlaylistViewModel = new SecondaryPlaylistViewModel { Title = artist.Name, CoverUri = artist.CoverUri, Subtitle = ToSubtitleString(artist.Albums?.Count ?? 0, artist.SongsCount), Albums = artist.Albums };
        }

        private string ToSubtitleString(int albumsCount, int songsCount, uint year = 0)
        {
            var AlbumsText = App.ResourceLoader.GetString("Text_Albums");
            var AlbumText = App.ResourceLoader.GetString("Text_Album");
            var SongsText = App.ResourceLoader.GetString("Text_Songs");
            var SongText = App.ResourceLoader.GetString("Text_Song");

            var albumsText = albumsCount > 1 ? AlbumsText : AlbumText;
            var songsText = songsCount > 1 ? SongsText : SongText;

            var text = albumsCount > 0 ? $"{albumsCount} {albumsText}, {songsCount} {songsText}" : $"{songsCount} {songsText}";
            return year > 0 ? $"{text}, {year}" : text;
        }

        public void AlbumsOnItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            if (album == null) return;

            ViewModelLocator.Instance.SecondaryPlaylistViewModel = new SecondaryPlaylistViewModel { Title = album.Title, CoverUri = album.CoverUri, Subtitle = ToSubtitleString(0, album.Songs?.Count ?? 0, album.Year), Songs = album.Songs };
        }

        public void GenresOnItemClick(object sender, ItemClickEventArgs e)
        {
            var genre = e.ClickedItem as Genre;
            if (genre == null) return;

            var songs = new List<Song>(genre.SongsCount);
            for (int i = 0; i < genre.SongsCount; i++)
            {
                var s = genre.Songs[i].Clone();
                s.TrackNumber = (uint)(i + 1);
                songs.Add(s);
            }

            ViewModelLocator.Instance.SecondaryPlaylistViewModel = new SecondaryPlaylistViewModel { Title = genre.Title, CoverUri = genre.CoverUri, Subtitle = ToSubtitleString(0, genre.SongsCount), Songs = songs, ShowArtistName = true };
        }
    }
}

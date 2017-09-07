using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Ayane.FrameworkEx;
using Newtonsoft.Json;
using SQLite.Net.Attributes;
using TagLib;

namespace Ayane.Models
{
    [Table("Songs")]
    partial class Song
    {
        [Column(nameof(Id))]
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [Column(nameof(Title))]
        public string Title { get; set; }

        [Column(nameof(ArtistName))]
        [Indexed]
        public string ArtistName { get; set; }

        [Column(nameof(AlbumTitle))]
        [Indexed]
        public string AlbumTitle { get; set; }

        [Column(nameof(GenreTitle))]
        [Indexed]
        public string GenreTitle { get; set; }

        [Column(nameof(TrackNumber))]
        public uint TrackNumber { get; set; }

        [Column(nameof(Year))]
        public uint Year { get; set; }

        [Column(nameof(DurationSeconds))]
        public double DurationSeconds { get; set; }

        [Ignore]
        public TimeSpan Duration
        {
            get { return TimeSpan.FromSeconds(DurationSeconds); }
            set { DurationSeconds = value.TotalSeconds; }
        }

        [Column(nameof(FileUriPath))]
        public string FileUriPath { get; set; }

        [Column(nameof(CoverFilePath))]
        public string CoverFilePath { get; set; }

        [Column(nameof(SongType))]
        private string SongType { get; set; }

        [Ignore]
        public SongStorageType StorageType
        {
            get
            {
                SongStorageType type;
                Enum.TryParse(SongType, true, out type);
                return type;
            }
            set { SongType = value.ToString(); }
        }

        [Ignore]
        public string FormatType => Path.GetExtension(FileUriPath);

        [Column(nameof(Remarks))]
        public string Remarks { get; set; }

        /// <summary>
        /// File access permission token. Required by WinRT
        /// If it is null, means it is located in MusicLibrary folder
        /// </summary>
        [Column(nameof(Token))]
        public string Token { get; set; }

        [Ignore]
        [JsonIgnore]
        public Uri CoverUri => string.IsNullOrEmpty(CoverFilePath) ? null : new Uri(CoverFilePath);

        [Ignore]
        [JsonIgnore]
        public Album Album { get; set; }

        [Ignore]
        [JsonIgnore]
        public Artist Artist { get; set; }

        /// <summary>
        /// For SQLite
        /// </summary>
        public Song()
        {
        }

        public Song(StorageFile file, string token)
        {
            const string unknown = "Unknown";

            FileUriPath = file.Path;
            Token = token;
            StorageType = SongStorageType.Local;

            using (var stream = file.OpenStreamForReadAsync().GetAwaiter().GetResult())
            {
                var tag = TagLib.File.Create(new StreamFileAbstraction(file.Name, stream, stream)).Tag;
                Title = tag.Title ?? unknown;
                AlbumTitle = tag.Album ?? unknown;
                ArtistName = tag.Performers.Length > 0 ? tag.Performers.Aggregate((c, n) => $"{c}; {n}").TrimEnd("; ") : unknown;
                GenreTitle = tag.FirstGenre ?? unknown;
                TrackNumber = tag.Track;
                Year = tag.Year;

                var musicProperties = file.Properties.GetMusicPropertiesAsync().GetAwaiter().GetResult();
                Duration = musicProperties.Duration;

                var cover = tag.Pictures.FirstOrDefault();
                if (SaveCover(cover?.Data.Data)) CoverFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, CoverFolder, FileUriPath.ComputeMD5());

            }

            Title = Title == unknown ? file.DisplayName : Title;
        }

        public override bool Equals(object obj)
        {
            var otherSong = obj as Song;
            return otherSong != null && FileUriPath.Equals(otherSong.FileUriPath, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"{ArtistName} - {Title}";
        }

        public Song Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Song>(json);
        }
    }

    partial class Song
    {
        /// <summary>
        /// Covert domain object to Windows Platform Object. If it returns null, means the file is not existing.
        /// </summary>
        /// <returns></returns>
        public async Task<MediaPlaybackItem> ToMediaPlaybackItem()
        {
            try
            {
                switch (StorageType)
                {
                    case SongStorageType.Local:
                        var file = await ToStorageFileAsync();
                        return file == null ? null : new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
                    case SongStorageType.Online:
                        return new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(FileUriPath)));
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }

        }

        public virtual async Task<StorageFile> ToStorageFileAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Token)) return await StorageFile.GetFileFromPathAsync(FileUriPath);

                var storageItem = await TokensManager.Instance.GetStorageItemAsync(Token);

                var file = storageItem as StorageFile;
                if (file != null)
                {
                    return file;
                }

                var folder = storageItem as StorageFolder;
                if (folder == null) return null;

                return await folder.GetFileAsync(Path.GetFileName(FileUriPath));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// This method should be called when deleteing playlist
        /// </summary>
        /// <returns></returns>
        public void RemoveToken()
        {
            if (string.IsNullOrEmpty(Token)) return;
            TokensManager.Instance.Remove(Token);
        }
    }

    partial class Song
    {
        private const string CoverFolder = "Covers";

        private bool SaveCover(byte[] data)
        {
            if (data == null) return false;

            try
            {
                var folder = ApplicationData.Current.LocalFolder.CreateFolderAsync(CoverFolder, CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
                var file = folder.CreateFileAsync(FileUriPath.ComputeMD5(), CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
                FileIO.WriteBytesAsync(file, data).GetAwaiter().GetResult();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException());
                return false;
            }
        }
    }

    class TempSong : Song
    {
        private readonly StorageFile _file;

        public TempSong(StorageFile file) : base(file, null)
        {
            _file = file;
        }

        public override async Task<StorageFile> ToStorageFileAsync()
        {
            return _file;
        }
    }

    enum SongStorageType
    {
        Local,
        Online,
    }
}

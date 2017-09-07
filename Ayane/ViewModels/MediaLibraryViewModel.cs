using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Ayane.Common;
using Ayane.FrameworkEx;
using Ayane.Models;
using Ayane.Widgets;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Ayane.ViewModels
{
    partial class MediaLibraryViewModel : CoreViewModel
    {
        private bool _initialized;
        private readonly List<PlaylistViewModel> _playlistsCache = new List<PlaylistViewModel>();

        public MediaLibraryViewModel()
        {
            DeletePlaylistCommand = new ActionCommand<string>(ExecuteDeleteingPlaylist);
            SyncPlaylistWithFoldersCommand = new ActionCommand<string>(ExecuteSyncPlaylistWithFolders);
            PlayerViewModel.ActiveSongChanged += PlayerViewModelOnActiveSongChanged;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;

            if (LocalSettingsHelper.LoadValue(CommonKeys.KeepScreenOn, false)) DisplayRequestHelper.RequestActive();

            PlaylistsTitle = new ObservableCollection<string>((await Playlist.GetPlaylistsTitleAsync()));

            var lastUsedPlaylist = LocalSettingsHelper.LoadValue(CommonKeys.LastUsedPlaylist, PlaylistsTitle.FirstOrDefault());
            _selectedPlaylistTitle = PlaylistsTitle.SingleOrDefault(p => p.Equals(lastUsedPlaylist, StringComparison.OrdinalIgnoreCase)) ?? PlaylistsTitle.FirstOrDefault();
            RaisePropertyChanged(nameof(SelectedPlaylistTitle));
            await SwitchPlaylist(SelectedPlaylistTitle);
            if (ActivePlaylistViewModel == null) return;

            var lastPlayedSong = LocalSettingsHelper.LoadValue(CommonKeys.LastPlayedSong, string.Empty);
            if (string.IsNullOrEmpty(lastPlayedSong)) return;

            var songs = ActivePlaylistViewModel.GetFlatSongs();
            if (songs.Count == 0) return;
            var targetSong = songs.FirstOrDefault(s => s.FileUriPath.Equals(lastPlayedSong, StringComparison.OrdinalIgnoreCase));
            if (targetSong == null) return;

            PlayerViewModel.PlaylistTitle = ActivePlaylistViewModel.Title;

            if (((App)Application.Current).IsFileActivated) return;

            PlayerViewModel.AutoPlay = LocalSettingsHelper.LoadValue(CommonKeys.AutoPlay, false);
            PlayerViewModel.Play(songs, songs.IndexOf(targetSong));
        }

        private NewPlaylistViewModel _temporaryPlaylistViewModel;
        public NewPlaylistViewModel TemporaryPlaylistViewModel
        {
            get
            {
                if (_temporaryPlaylistViewModel != null) return _temporaryPlaylistViewModel;
                _temporaryPlaylistViewModel = new NewPlaylistViewModel();
                _temporaryPlaylistViewModel.Created += TemporaryPlaylist_OnCreated;
                return _temporaryPlaylistViewModel;
            }
        }

        private async void TemporaryPlaylist_OnCreated(object sender, NewPlaylistViewModel.CreatePlaylistEventArgs e)
        {
            var vm = (NewPlaylistViewModel)sender;
            _initialized = PlaylistsTitle.Count > 0;

            vm.Created -= TemporaryPlaylist_OnCreated;
            _temporaryPlaylistViewModel = null;
            PlaylistsTitle.Add(e.Playlist.Title);
            RaisePropertyChanged(nameof(HasAnyPlaylist));
            e.Playlist.Dispose();

            if (_initialized) return;
            await InitializeAsync();
        }

        public bool HasAnyPlaylist => PlaylistsTitle.Count > 0;

        private ObservableCollection<string> _playlistsTitle;

        public ObservableCollection<string> PlaylistsTitle
        {
            get { return _playlistsTitle; }
            set
            {
                _playlistsTitle = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// This property is main pivot for switching playlist
        /// </summary>
        private string _selectedPlaylistTitle;

        public string SelectedPlaylistTitle
        {
            get { return _selectedPlaylistTitle; }
            set
            {
                if (_selectedPlaylistTitle == value) return;
                _selectedPlaylistTitle = value;
                RaisePropertyChanged();

                SwitchPlaylist(value);
                LocalSettingsHelper.SaveValue(CommonKeys.LastUsedPlaylist, value);
            }
        }

        private PlaylistViewModel _activePlaylistViewModel;

        public PlaylistViewModel ActivePlaylistViewModel
        {
            get { return _activePlaylistViewModel; }
            private set
            {
                _activePlaylistViewModel = value;
                RaisePropertyChanged();
            }
        }

        public PlayerViewModel PlayerViewModel => ViewModelLocator.Instance.PlayerViewModel;

        private async Task SwitchPlaylist(string title)
        {
            if (string.IsNullOrEmpty(title)) return;
            var cachedPlaylist = _playlistsCache.SingleOrDefault(s => s.Title.Equals(title));
            if (cachedPlaylist != null)
            {
                ActivePlaylistViewModel = cachedPlaylist;
                return;
            }

            try
            {
                ActivePlaylistViewModel = await Task.Run(() => new PlaylistViewModel(title));
                _playlistsCache.Add(ActivePlaylistViewModel);
            }
            catch (Exception)
            {
                ActivePlaylistViewModel = null;
            }
        }
    }

    partial class MediaLibraryViewModel
    {
        public ICommand DeletePlaylistCommand { get; private set; }
        public ICommand SyncPlaylistWithFoldersCommand { get; private set; }

        private void ExecuteDeleteingPlaylist(string s)
        {
            var title = PlaylistsTitle.SingleOrDefault(p => p.Equals(s, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(title)) return;

            var msgBox = new MessageBox
            {
                PositiveButtonTitle = App.ResourceLoader.GetString("MessageBox_Title_PositiveButton"),
                NegativeButtonTitle = App.ResourceLoader.GetString("MessageBox_Title_NegativeButton"),
                Title = App.ResourceLoader.GetString("MessageBox_Title_DeletePlaylist"),
                Message = App.ResourceLoader.GetString("MessageBox_Message_DeletePlaylist"),
            };

            msgBox.PositiveButtonClick += async (sender, args) =>
            {
                PlaylistsTitle.Remove(title);
                RaisePropertyChanged(nameof(HasAnyPlaylist));

                var cachedPlaylistVm = _playlistsCache.SingleOrDefault(v => v.Title.Equals(title));

                if (cachedPlaylistVm == ActivePlaylistViewModel)
                {
                    if (HasAnyPlaylist)
                        SelectedPlaylistTitle = PlaylistsTitle.First();
                    else
                        ActivePlaylistViewModel = null;
                }

                _playlistsCache.Remove(cachedPlaylistVm);

                if (cachedPlaylistVm != null)
                {
                    cachedPlaylistVm.DeletePlaylist();
                }
                else
                {
                    await Playlist.DeleteAsync(title);
                }

            };
            msgBox.Show();
        }

        private async void ExecuteSyncPlaylistWithFolders(string title)
        {
            var playlistVm = _playlistsCache.SingleOrDefault(s => s.Title.Equals(title));
            if (playlistVm == null)
            {
                playlistVm = await Task.Run(() => new PlaylistViewModel(title));
                _playlistsCache.Add(playlistVm);
            }

            await playlistVm.Sync();
        }

        public void OnNewPlaylistDragOver(object sender, DragEventArgs e)
        {
            if (CheckTokensThresold())
            {
                e.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        public async void OnNewPlaylistDrop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            var items = await e.DataView.GetStorageItemsAsync();

            var folders = items.OfType<StorageFolder>().ToList();
            if (!folders.Any()) return;

            foreach (var folder in folders)
            {
                if (!await Playlist.IsTitleAvailable(folder.DisplayName))
                {
                    Toast.ShowMessage(App.ResourceLoader.GetString("Message_PlaylistTitleExist"));
                    return;
                }

                var newplaylistVm = new NewPlaylistViewModel { Title = folder.DisplayName };
                newplaylistVm.Created += TemporaryPlaylist_OnCreated;

                var messageTip = newplaylistVm.CreateProcessingMessageBox();
                messageTip.Show();

                newplaylistVm.AddStoageItems(new[] { folder });
                await newplaylistVm.CreatePlaylistAsync();

                messageTip.Close();
            }
        }
    }

    partial class MediaLibraryViewModel
    {
        private void PlayerViewModelOnActiveSongChanged(object sender, EventArgs eventArgs)
        {
            var song = PlayerViewModel.ActiveSong;
            if (song == null) return;
            LocalSettingsHelper.SaveValue(CommonKeys.LastPlayedSong, song.FileUriPath);
        }

        public static bool CheckTokensThresold()
        {
            if (TokensManager.Instance.Count <= 999) return false;
            var msgBox = new MessageBox
            {
                Title = App.ResourceLoader.GetString("MessageBox_Title_Warning"),
                Message = App.ResourceLoader.GetString("MessageBox_Message_TokensFull"),
            };
            msgBox.Show();
            return true;
        }

        public static void CheckTokensUsage()
        {
            if (TokensManager.Instance.Count <= 900) return;
            if (LocalSettingsHelper.LoadValue(CommonKeys.TokensTipUsed, false)) return;

            var msgBox = new MessageBox
            {
                Title = App.ResourceLoader.GetString("MessageBox_Title_Warning"),
                Message = string.Format(App.ResourceLoader.GetString("MessageBox_Message_TokensAlmostFull"), TokensManager.Instance.Count),
                PositiveButtonTitle = App.ResourceLoader.GetString("MessageBox_Title_PositiveButton"),
                NegativeButtonTitle = App.ResourceLoader.GetString("MessageBox_Title_NegativeButton"),
            };

            msgBox.PositiveButtonClick += (sender, args) => LocalSettingsHelper.SaveValue(CommonKeys.TokensTipUsed, true);
            msgBox.Show();
        }
    }
}

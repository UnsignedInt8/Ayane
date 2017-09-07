using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Ayane.Models;
using GalaSoft.MvvmLight;

namespace Ayane.ViewModels
{
    class SecondaryPlaylistViewModel : ViewModelBase
    {
        private string _title;
        private string _subtitle;
        private Uri _coverUri;

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;
                _title = value;
                RaisePropertyChanged();
            }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set
            {
                if (_subtitle == value) return;
                _subtitle = value;
                RaisePropertyChanged();
            }
        }

        public Uri CoverUri
        {
            get { return _coverUri; }
            set
            {
                if (_coverUri == value) return;
                _coverUri = value;
                RaisePropertyChanged();
            }
        }

        public IList<Album> Albums { get; set; }
        public IList<Song> Songs { get; set; }

        public int SongsCount => Songs?.Count ?? (Albums?.Sum(a => a.Songs?.Count ?? 0) ?? 0);

        public bool ShowArtistName { get; set; } = false;

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

            var playerVm = ViewModelLocator.Instance.PlayerViewModel;
            playerVm.PlaylistTitle = ViewModelLocator.Instance.MediaLibraryViewModel.ActivePlaylistViewModel?.Title ?? Title;

            var songs = Songs ?? Albums?.SelectMany(a => a.Songs).ToList();
            if (songs == null) return;

            playerVm.AutoPlay = true;
            playerVm.Play(songs, sender.SelectedIndex, force);
        }

    }
}

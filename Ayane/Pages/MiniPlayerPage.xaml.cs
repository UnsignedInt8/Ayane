using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Ayane.Common;
using Ayane.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MiniPlayerPage : Page
    {
        public MiniPlayerPage()
        {
            InitializeComponent();
            ViewModel = ViewModelLocator.Instance.PlayerViewModel;
            ViewModel.SkipToNext += ViewModelOnSkipToNext;
            ViewModel.SkipToPrevious += ViewModelOnSkipToPrevious;
            ViewModel.ShuffleChanged += ViewModelOnShuffleChanged;
            //ViewModel.ActiveSongChanged += ViewModelOnActiveSongChanged;

            Cover.PointerWheelChanged += OnPointerWheelChanged;
            Unloaded += OnUnloaded;
        }

        //private void ViewModelOnActiveSongChanged(object sender, EventArgs eventArgs)
        //{
        //    Cover.CurrentUri = ViewModel.ActiveSong?.CoverUri;
        //}

        private void ViewModelOnShuffleChanged(object sender, EventArgs eventArgs)
        {
            Cover.CurrentUri = ViewModel.ActiveSong?.CoverUri;
            Cover.PreviousUri = ViewModel.PreviousSong?.CoverUri;
            Cover.NextUri = ViewModel.NextSong?.CoverUri;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel.SkipToNext -= ViewModelOnSkipToNext;
            ViewModel.SkipToPrevious -= ViewModelOnSkipToPrevious;
            ViewModel.ShuffleChanged -= ViewModelOnShuffleChanged;
        }

        private void ViewModelOnSkipToPrevious(object sender, EventArgs eventArgs)
        {
            var currentUri = ViewModel.ActiveSong?.CoverUri;
            var nextUri = ViewModel.NextSong?.CoverUri;
            var previousUri = ViewModel.PreviousSong?.CoverUri;

            if (currentUri == Cover.CurrentUri) return;

            Cover.SkipToPrevious(() =>
            {
                Cover.CurrentUri = currentUri;
                Cover.NextUri = nextUri;
                Cover.PreviousUri = previousUri;
            });

        }

        private void ViewModelOnSkipToNext(object sender, EventArgs eventArgs)
        {
            var currentUri = ViewModel.ActiveSong?.CoverUri;
            var nextUri = ViewModel.NextSong?.CoverUri;
            var previousUri = ViewModel.PreviousSong?.CoverUri;

            if (currentUri == Cover.CurrentUri) return;

            Cover.SkipToNext(() =>
            {
                Cover.CurrentUri = currentUri;
                Cover.NextUri = nextUri;
                Cover.PreviousUri = previousUri;
            });
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            if (args.GetCurrentPoint(this).Properties.MouseWheelDelta >= -20) return;
            if (HideAnimation.GetCurrentState() == ClockState.Active) return;
            Hide();
        }

        private void OnHiding()
        {
            Hiding?.Invoke(this, EventArgs.Empty);
        }

        private void OnHidingCompleted()
        {
            HidingCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void HideAnimation_OnCompleted(object sender, object e)
        {
            OnHidingCompleted();
        }

        private void BackToWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private PlayerViewModel ViewModel { get; set; }

        public event EventHandler Hiding;
        public event EventHandler HidingCompleted;

        public async void Show()
        {
            Cover.CurrentUri = ViewModel.ActiveSong?.CoverUri;
            Cover.PreviousUri = ViewModel.PreviousSong?.CoverUri;
            Cover.NextUri = ViewModel.NextSong?.CoverUri;

            await SetupCircleCoverAsync();

            Window.Current.SetTitleBar(TitleBarHolder);
            App.SetTitleBarInDarkBackground();
            ShowAnimation.Begin();

            ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.SpotlightTransition)?.TryStart(CircleCover);
        }

        public async void Hide()
        {
            await SetupCircleCoverAsync();

            if (ViewModel.ActiveSong != null)
            {
                ConnectedAnimationService.GetForCurrentView().DefaultDuration = TimeSpan.FromMilliseconds(500);
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.SpotlightTransition, CircleCover);
            }

            OnHiding();
            HideAnimation.Begin();
        }

        private async Task SetupCircleCoverAsync()
        {
            var song = ViewModel.ActiveSong;
            if (song == null || song.CoverUri == null)
            {
                CircleCover.Fill = new SolidColorBrush(Colors.Transparent);
                return;
            }

            if (ReferenceEquals(CircleCover.Tag, song.CoverUri)) return;

            try
            {
                var coverFile = await StorageFile.GetFileFromPathAsync(song.CoverUri.OriginalString);
                using (var stream = await coverFile.OpenReadAsync())
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    CircleCover.Fill = new ImageBrush { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
                }
            }
            catch (Exception)
            {

            }

        }
    }
}

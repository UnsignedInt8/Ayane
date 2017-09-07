using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Ayane.Annotations;
using Ayane.Common;
using Ayane.FrameworkEx;
using Ayane.ViewModels;
using Ayane.Widgets;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            InitializeComponent();
            Window.Current.SetTitleBar(TitleBarHolder);
            Spotlight.Hide();

            ViewModel = ViewModelLocator.Instance.MediaLibraryViewModel;
            ContentFrame.Navigated += ContentFrameOnNavigated;
            ContentFrame.Navigate(typeof(PlaylistTopContentPage));
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            Spotlight.PointerWheelChanged += SpotlightOnPointerWheelChanged;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            HamburgerButton_OnBackClicked(null, null);
            args.Handled = true;
        }

        private void ContentFrameOnNavigated(object sender, NavigationEventArgs args)
        {
            if (ContentFrame.BackStackDepth > 0)
            {
                HamburgerButton.ShowBack();
                if (ContentFrame.Content is SongsCollectionPage) AppTitleTextBlock.Foreground = HamburgerButton.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                AppTitleTextBlock.Text = App.ResourceLoader.GetString("Text_Title_App");
                HamburgerButton.ShowMenu();
                AppTitleTextBlock.Foreground = HamburgerButton.Foreground = App.Current.Resources["TitleBarForegroundBrush"] as SolidColorBrush;
                Spotlight.Show(true);
                App.ResetTitleBarToAccentColor();
            }
        }

        private async void HamburgerButton_OnBackClicked(object sender, EventArgs e)
        {
            if (!ContentFrame.CanGoBack) return;

            var songsCollectionPage = ContentFrame.Content as SongsCollectionPage;
            if (songsCollectionPage != null)
            {
                songsCollectionPage.PrepareTransition();
                await Task.Delay(TimeSpan.FromMilliseconds(126));
            }

            ContentFrame.GoBack();
        }

        private MediaLibraryViewModel _viewModel;
        private MediaLibraryViewModel ViewModel { get { return _viewModel; } set { _viewModel = value; OnPropertyChanged(); } }

        PlayerViewModel PlayerViewModel => ViewModelLocator.Instance.PlayerViewModel;

        private void HamburgBackButton_OnMenuClicked(object sender, EventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;

            if (SplitView.IsPaneOpen) Spotlight.Hide();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Drawer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SplitView.IsPaneOpen = false;
        }

        private void SplitView_OnPaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            if (ContentFrame.BackStackDepth > 0) return;
            Spotlight.Show();
        }

        private void CreatePlaylistButton_OnClick(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(CreatePlaylistPage));
            SplitView.IsPaneOpen = false;
            Spotlight.Hide(true);
        }

        private void SleepingMode_OnClick(object sender, RoutedEventArgs e)
        {
            AppTitleTextBlock.Text = App.ResourceLoader.GetString("Text_Title_SleepingMode");
            ContentFrame.Navigate(typeof(SleepingModePage));
            SplitView.IsPaneOpen = false;
            Spotlight.Hide(true);
        }

        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            AppTitleTextBlock.Text = App.ResourceLoader.GetString("Text_Title_Settings");
            ContentFrame.Navigate(typeof(SettingsPage));
            SplitView.IsPaneOpen = false;
            Spotlight.Hide(true);
        }

        private void AudioEffectsButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppTitleTextBlock.Text = App.ResourceLoader.GetString("Text_Title_AudioEffects");
            ContentFrame.Navigate(typeof(AudioEffectsPage));
            SplitView.IsPaneOpen = false;
            Spotlight.Hide(true);
        }

        private void HelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppTitleTextBlock.Text = App.ResourceLoader.GetString("Text_Title_Help");
            ContentFrame.Navigate(typeof(HelpPage));
            SplitView.IsPaneOpen = false;
            Spotlight.Hide(true);
        }
    }

    /// <summary>
    /// For Spotlight and PlayerPage
    /// </summary>
    partial class MainPage
    {
        private void SpotlightOnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            if (args.GetCurrentPoint(Spotlight).Properties.MouseWheelDelta < 20) return;
            if (PlayerPage.Visibility == Visibility.Visible) return;
            TransitionToPlayer();
        }

        private void TransitionToPlayer()
        {
            ConnectedAnimationService.GetForCurrentView().DefaultDuration = TimeSpan.FromMilliseconds(250);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.SpotlightTransition, Spotlight.XamlCover);
            PlayerPage.Visibility = Visibility.Visible;
            PlayerPage.IsHitTestVisible = true;
            PlayerPage.Show();
        }

        private void Spotlight_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            TransitionToPlayer();
        }

        private void PlayerPage_OnHiding(object sender, EventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.SpotlightTransition)?.TryStart(Spotlight.XamlCover);
        }

        private void PlayerPage_OnHidingCompleted(object sender, EventArgs e)
        {
            PlayerPage.IsHitTestVisible = false;
            PlayerPage.Visibility = Visibility.Collapsed;
            Window.Current.SetTitleBar(TitleBarHolder);
            if (ContentFrame.Content is SongsCollectionPage) return;
            App.ResetTitleBarToAccentColor();
        }

    }
}

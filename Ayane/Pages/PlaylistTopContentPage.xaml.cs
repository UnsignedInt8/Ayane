using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Ayane.Annotations;
using Ayane.Common;
using Ayane.Controls;
using Ayane.FrameworkEx;
using Ayane.ViewModels;
using Ayane.Widgets;
using Microsoft.Toolkit.Uwp.UI;
using WinRTXamlToolkit.AwaitableUI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistTopContentPage : Page, INotifyPropertyChanged
    {
        public PlaylistTopContentPage()
        {
            InitializeComponent();
            if (DesignMode.DesignModeEnabled) return;

            ArtistsListView.ItemClick += CollectionListView_OnItemClick;
            AlbumsGridView.ItemClick += AlbumsGridViewOnItemClick;
            GenresListView.ItemClick += CollectionListView_OnItemClick;
            DataContextChanged += OnDataContextChanged;
            Window.Current.Activated += WindowOnActivated;
        }

        private void WindowOnActivated(object sender, WindowActivatedEventArgs windowActivatedEventArgs)
        {
            if (windowActivatedEventArgs.WindowActivationState != CoreWindowActivationState.CodeActivated) return;
            SongsListViewJumpToActiveSong();
        }

        private void SongsListViewJumpToActiveSong()
        {
            var activeSong = PlayerViewModel.ActiveSong;
            if (activeSong == null) return;

            SongsListView.ScrollIntoView(activeSong, ScrollIntoViewAlignment.Leading);
        }

        private async void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            SwitchToZoomedInView();

            if (args.NewValue == ViewModel) return;

            if (!(DataContext is PlaylistViewModel))
            {
                SongsListView.ItemsSource = null;
                ((ListViewBase)SongsSemanticZoom.ZoomedOutView).ItemsSource = null;
                ViewModel = null;
                return;
            }

            ViewModel = (PlaylistViewModel)DataContext;

            var coll = new CollectionViewSource
            {
                IsSourceGrouped = true,
                Source = ViewModel.Songs,
            };

            SongsListView.ItemsSource = coll.View;
            ((ListViewBase)SongsSemanticZoom.ZoomedOutView).ItemsSource = coll.View.CollectionGroups;

            var itemsView = new[] { ArtistsListView, ((ListViewBase)ArtistsSemanticZoom.ZoomedOutView), AlbumsGridView, ((ListViewBase)AlbumsSemanticZoom.ZoomedOutView), GenresListView, ((ListViewBase)GenresSemanticZoom.ZoomedOutView) };
            foreach (var listViewBase in itemsView)
            {
                listViewBase.ItemsSource = null;
            }
            
            await ReconnectPivotItemDataSourceAsync();

            SongsListViewJumpToActiveSong();
        }

        PlaylistViewModel _viewModel;
        PlaylistViewModel ViewModel { get { return _viewModel; } set { _viewModel = value; OnPropertyChanged(); } }

        private PlayerViewModel PlayerViewModel => ViewModelLocator.Instance.PlayerViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SwitchToZoomedInView();
            await ReconnectPivotItemDataSourceAsync();
        }

        private void SwitchToZoomedInView()
        {
            var zooms = new[] { SongsSemanticZoom, ArtistsSemanticZoom, AlbumsSemanticZoom, GenresSemanticZoom };
            foreach (var semanticZoom in zooms)
            {
                semanticZoom.IsZoomedInViewActive = true;
            }
        }

        private async Task ReconnectPivotItemDataSourceAsync()
        {
            switch (Skeleton.SelectedIndex)
            {
                case 1:
                    if (ArtistsListView.ItemsSource != null) return;
                    await RefreshArtistsDataSourceAsync();
                    break;
                case 2:
                    if (AlbumsGridView.ItemsSource != null) return;
                    await RefreshAlbumsDataSourceAsync();
                    break;
                case 3:
                    if (GenresListView.ItemsSource != null) return;
                    await RefreshGenresDataSourceAsync();
                    break;
            }
        }

        private async Task RefreshArtistsDataSourceAsync()
        {
            if (ViewModel == null) return;
            await ViewModel.RefreshArtistsAsync();
            var coll = new CollectionViewSource
            {
                IsSourceGrouped = true,
                Source = ViewModel.Artists,
            };
            ArtistsListView.ItemsSource = coll.View;
            ((ListViewBase)ArtistsSemanticZoom.ZoomedOutView).ItemsSource = coll.View.CollectionGroups;
        }

        private async Task RefreshAlbumsDataSourceAsync()
        {
            if (ViewModel == null) return;
            await ViewModel.RefreshAlbumsAsync();
            var coll = new CollectionViewSource
            {
                IsSourceGrouped = true,
                Source = ViewModel.Albums,
            };

            AlbumsGridView.ItemsSource = coll.View;
            ((ListViewBase)AlbumsSemanticZoom.ZoomedOutView).ItemsSource = coll.View.CollectionGroups;
        }

        private async Task RefreshGenresDataSourceAsync()
        {
            if (ViewModel == null) return;
            await ViewModel.RefreshGenresAsync();
            var coll = new CollectionViewSource
            {
                IsSourceGrouped = true,
                Source = ViewModel.Genres,
            };

            GenresListView.ItemsSource = coll.View;
            ((ListViewBase)GenresSemanticZoom.ZoomedOutView).ItemsSource = coll.View.CollectionGroups;
        }

        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            GenresListView.ItemsSource = AlbumsGridView.ItemsSource = ArtistsListView.ItemsSource = null;
        }
    }

    /// <summary>
    /// Page Transition
    /// </summary>
    partial class PlaylistTopContentPage
    {

        private object _transitionItem;

        private void RunConnectedAnimation(UIElement cover, UIElement title = null, UIElement subtitle = null)
        {
            if (title != null) ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.TitleTransition)?.TryStart(title);
            if (subtitle != null) ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.SubtitleTransition)?.TryStart(subtitle);
            ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.CoverTransition)?.TryStart(cover);
        }

        private void PrepareAnimation(UIElement cover, UIElement title = null, UIElement subtitle = null)
        {
            ConnectedAnimationService.GetForCurrentView().DefaultDuration = TimeSpan.FromSeconds(0.5);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.CoverTransition, cover);
            if (title != null) ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.TitleTransition, title);
            if (subtitle != null) ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.SubtitleTransition, subtitle);
        }

        private void CollectionListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            _transitionItem = e.ClickedItem;
            var container = ((ListViewBase)sender).ContainerFromItem(e.ClickedItem);
            var widget = container.FindDescendant<SongsCollectionItem>();

            PrepareAnimation(widget.XamlCoverImage, widget.XamlTitleBlock, widget.XamlSubtitleTextBlock);
            ((ListViewBase)sender).Loaded += ArtistsListViewOnLoaded;

            Frame.Navigate(typeof(SongsCollectionPage), e.ClickedItem);
        }

        private void ArtistsListViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ((ListViewBase)sender).Loaded -= ArtistsListViewOnLoaded;
            if (_transitionItem == null) return;
            var listView = (ListViewBase)sender;
            var widget = listView.ContainerFromItem(_transitionItem)?.FindDescendant<SongsCollectionItem>();
            if (widget == null) return;
            RunConnectedAnimation(widget.XamlCoverImage, widget.XamlTitleBlock, widget.XamlSubtitleTextBlock);
        }

        private void AlbumsGridViewOnItemClick(object sender, ItemClickEventArgs e)
        {
            _transitionItem = e.ClickedItem;
            var container = ((ListViewBase)sender).ContainerFromItem(e.ClickedItem);
            var cover = container.FindDescendant<FadeImage>();
            var title = container.FindDescendant<TextBlock>();
            var mask = container.FindDescendant<Rectangle>();

            PrepareAnimation(cover, title, mask);

            AlbumsGridView.Loaded += AlbumsGridViewOnLoaded;
            Frame.Navigate(typeof(SongsCollectionPage));
        }

        private void AlbumsGridViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AlbumsGridView.Loaded -= AlbumsGridViewOnLoaded;
            if (_transitionItem == null) return;
            var lv = (ListViewBase)sender;
            var container = lv.ContainerFromItem(_transitionItem);
            if (container == null) return;
            var cover = container.FindDescendant<FadeImage>();
            var title = container.FindDescendant<TextBlock>();
            var mask = container.FindDescendant<Rectangle>();
            RunConnectedAnimation(cover, title, mask);
        }

    }
}

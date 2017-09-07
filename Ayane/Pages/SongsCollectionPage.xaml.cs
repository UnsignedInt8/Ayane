using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Ayane.Annotations;
using Ayane.Common;
using Ayane.FrameworkEx;
using Ayane.Models;
using Ayane.ViewModels;
using Microsoft.Toolkit.Uwp.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SongsCollectionPage : Page
    {
        public SongsCollectionPage()
        {
            InitializeComponent();
            ViewModel = DataContext as SecondaryPlaylistViewModel;
            SongsListView.Loaded += OnLoaded;

            AccentTitleTextBlock.GetVisual().Opacity = 0;
            SubtitleTextBlock.GetVisual().Opacity = 0;
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            SongsListView.Loaded -= OnLoaded;
            SetupAnimations();
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            SetupDataSource();
        }

        private SecondaryPlaylistViewModel ViewModel { get; set; }

        public void PrepareTransition()
        {
            CoverGrid.GetVisual().Scale = new System.Numerics.Vector3(1, 1, 1);
            TitlePanel.GetVisual().Offset = new System.Numerics.Vector3(0, 0, 0);
            SubtitleTextBlock.GetVisual().Opacity = 1;

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.CoverTransition, CoverGrid);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.TitleTransition, AccentTitleTextBlock);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKeys.SubtitleTransition, SubtitleTextBlock);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.SizeChanged -= WindowOnSizeChanged;
        }

        private void WindowOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            SetupPaddingFooter();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.SetTitleTransparent();
            Window.Current.SizeChanged += WindowOnSizeChanged;

            if (CoverImage.UriSource == null) CoverImageOnImageProcessed(null, null);
        }

        private void CoverImageOnImageProcessed(object sender, RoutedEventArgs routedEventArgs)
        {
            CoverImage.ImageProcessed -= CoverImageOnImageProcessed;
            var coverAnim = ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.CoverTransition);
            coverAnim?.TryStart(CoverGrid);

            var titleAnim = ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.TitleTransition);
            titleAnim?.TryStart(AccentTitleTextBlock);

            var subtitleAnim = ConnectedAnimationService.GetForCurrentView().GetAnimation(ConnectedAnimationKeys.SubtitleTransition);
            subtitleAnim?.TryStart(SubtitleTextBlock);

            CoverGrid.Opacity = 1;

            AccentTitleTextBlock.GetVisual().Opacity = 1;
            SubtitleTextBlock.GetVisual().Opacity = 1;
        }

        private void SetupDataSource()
        {
            if (ViewModel == null) return;

            try
            {
                if (ViewModel.Songs != null)
                {
                    SongsListView.ItemsSource = ViewModel.Songs;
                    return;
                }

                if (ViewModel.Albums == null) return;

                var coll = new CollectionViewSource
                {
                    IsSourceGrouped = true,
                    ItemsPath = new PropertyPath(nameof(Album.Songs)),
                    Source = ViewModel.Albums
                };

                SongsListView.ItemsSource = coll.View;
            }
            finally
            {
                SetupPaddingFooter();
            }

        }

        private void SetupPaddingFooter()
        {
            const float itemHeight = 45f;
            const float groupHeaderHeight = 32;

            var clientHeight = Window.Current.Bounds.Height - 32;
            var lowHeight = clientHeight - itemHeight;
            var highHeight = clientHeight + itemHeight;
            var itemsHeight = ViewModel.SongsCount * itemHeight + 64 + (ViewModel.Albums?.Count ?? 0) * groupHeaderHeight;

            if (itemsHeight > lowHeight && itemsHeight < highHeight)
            {
                ListViewPaddingFooter.Height = 108;
            }
            else
            {
                ListViewPaddingFooter.Height = 0;
            }
        }

        private void SetupAnimations()
        {
            const float maxOffset = -16f;

            var scrollViewer = SongsListView.FindDescendant<ScrollViewer>();
            var scrollerPropSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

            var coverVisual = CoverGrid.GetVisual();
            var coverPanelVisual = CoverPanel.GetVisual();
            var titlePanelVisual = TitlePanel.GetVisual();
            var contrastTitleTextVisual = ContrastTitleTextBlock.GetVisual();
            var accentTitleTextVisual = AccentTitleTextBlock.GetVisual();
            var subtitleTextVisual = SubtitleTextBlock.GetVisual();

            var c = this.GetVisual().Compositor;
            var coverPanelCoordinateY = c.CreateExpressionAnimation("Clamp(scroller.Translation.Y * factor, -16, 0)");
            coverPanelCoordinateY.SetReferenceParameter("scroller", scrollerPropSet);
            coverPanelCoordinateY.SetScalarParameter("factor", 0.2f);
            coverPanelVisual.StartAnimation("Offset.Y", coverPanelCoordinateY);

            var scaleAlgorithm = "Clamp(1 - layer.Offset.Y / maxOffset, 0.4, 1)";
            var scaleAnim = c.CreateExpressionAnimation(scaleAlgorithm);
            scaleAnim.SetReferenceParameter("layer", coverPanelVisual);
            scaleAnim.SetScalarParameter("maxOffset", maxOffset);
            coverVisual.StartAnimation("Scale.X", scaleAnim);
            coverVisual.StartAnimation("Scale.Y", scaleAnim);

            var titlePanelCoordinateXAnim = c.CreateExpressionAnimation("coverOriginWidth * (layer.Scale.X - 1.0)");
            titlePanelCoordinateXAnim.SetReferenceParameter("layer", coverVisual);
            titlePanelCoordinateXAnim.SetScalarParameter("coverOriginWidth", 76f);
            titlePanelVisual.StartAnimation("Offset.X", titlePanelCoordinateXAnim);

            var titlePanelCoordinateYAnim = c.CreateExpressionAnimation("layer.Offset.Y * 1.5");
            titlePanelCoordinateYAnim.SetReferenceParameter("layer", coverPanelVisual);
            titlePanelVisual.StartAnimation("Offset.Y", titlePanelCoordinateYAnim);

            //var titleTextColorAnim = c.CreateExpressionAnimation("layer.Offset.Y < maxOffset / 2 ? whiteColor : accentColor");
            //titleTextColorAnim.SetReferenceParameter("layer", coverPanelVisual);
            //titleTextColorAnim.SetScalarParameter("maxOffset", maxOffset);
            //titleTextColorAnim.SetColorParameter("whiteColor", Colors.White);
            //titleTextColorAnim.SetColorParameter("accentColor", ((SolidColorBrush)TitleTextBlock.Foreground).Color);

            var accentTitleOpacityAnim = c.CreateExpressionAnimation("1 - layer.Offset.Y / maxOffset");
            accentTitleOpacityAnim.SetReferenceParameter("layer", coverPanelVisual);
            accentTitleOpacityAnim.SetScalarParameter("maxOffset", maxOffset);
            accentTitleTextVisual.StartAnimation("Opacity", accentTitleOpacityAnim);

            var contrastTitleOpcityAnim = c.CreateExpressionAnimation("layer.Offset.Y / maxOffset");
            contrastTitleOpcityAnim.SetReferenceParameter("layer", coverPanelVisual);
            contrastTitleOpcityAnim.SetScalarParameter("maxOffset", maxOffset);
            contrastTitleTextVisual.StartAnimation("Opacity", contrastTitleOpcityAnim);

            var subtitleTextOpacityAnim = c.CreateExpressionAnimation("1 - layer.Offset.Y / maxOffset");
            subtitleTextOpacityAnim.SetReferenceParameter("layer", coverPanelVisual);
            subtitleTextOpacityAnim.SetScalarParameter("maxOffset", maxOffset);
            subtitleTextVisual.StartAnimation("Opacity", subtitleTextOpacityAnim);
        }
    }
}

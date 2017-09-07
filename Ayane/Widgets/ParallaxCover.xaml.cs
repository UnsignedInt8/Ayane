using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Widgets
{
    public sealed partial class ParallaxCover : UserControl
    {
        private const double SideImageFactor = 0.3;

        public ParallaxCover()
        {
            InitializeComponent();
            SizeChanged += (sender, args) => ResetBackImagesLayout();
            Loaded += (sender, args) => ResetBackImagesLayout();
            Left.DecodePixelHeight = Center.DecodePixelHeight = Right.DecodePixelHeight = Left.DecodePixelWidth = Center.DecodePixelWidth = Right.DecodePixelWidth = -1;
        }

        private void ResetBackImagesLayout()
        {
            ((CompositeTransform)Right.RenderTransform).TranslateX = SideImageFactor * ActualWidth;
            ((CompositeTransform)Left.RenderTransform).TranslateX = -SideImageFactor * ActualWidth;
        }

        public ImageSource CenterImage => Center.Source;
        public ICommand SkipNextCommand { get { return GetValue(SkipNextProperty) as ICommand; } set { SetValue(SkipNextProperty, value); } }
        public ICommand SkipPreviousCommand { get { return GetValue(SkipPreviousProperty) as ICommand; } set { SetValue(SkipPreviousProperty, value); } }
        public Uri PreviousUri { get { return GetValue(PreviousUriProperty) as Uri; } set { SetValue(PreviousUriProperty, value); } }
        public Uri CurrentUri { get { return GetValue(CurrentUriProperty) as Uri; } set { SetValue(CurrentUriProperty, value); } }
        public Uri NextUri { get { return GetValue(NextUriProperty) as Uri; } set { SetValue(NextUriProperty, value); } }

        public static readonly DependencyProperty SkipNextProperty = DependencyProperty.Register(nameof(SkipNextCommand), typeof(ICommand), typeof(ParallaxCover), null);
        public static readonly DependencyProperty SkipPreviousProperty = DependencyProperty.Register(nameof(SkipPreviousCommand), typeof(ICommand), typeof(ParallaxCover), null);
        public static readonly DependencyProperty PreviousUriProperty = DependencyProperty.Register(nameof(PreviousUri), typeof(Uri), typeof(ParallaxCover), new PropertyMetadata(null, (o, args) => ((ParallaxCover)o).Left.UriSource = args.NewValue as Uri));
        public static readonly DependencyProperty CurrentUriProperty = DependencyProperty.Register(nameof(CurrentUri), typeof(Uri), typeof(ParallaxCover), new PropertyMetadata(null, (o, args) => ((ParallaxCover)o).Center.UriSource = args.NewValue as Uri));
        public static readonly DependencyProperty NextUriProperty = DependencyProperty.Register(nameof(NextUri), typeof(Uri), typeof(ParallaxCover), new PropertyMetadata(null, (o, args) => ((ParallaxCover)o).Right.UriSource = args.NewValue as Uri));

        private async void Center_OnInteractionCompleted(object sender, EventArgs e)
        {
            if (Math.Abs(Center.OffsetX) > .55 * ActualWidth)
            {
                if (Center.OffsetX < 0)
                {
                    SkipToNext(() =>
                    {
                        PreviousUri = CurrentUri;
                        CurrentUri = NextUri;
                        NextUri = null;
                    });
                    SkipNextCommand?.Execute(null);
                }
                else if (Center.OffsetX > 0)
                {
                    SkipToPrevious(() =>
                    {
                        NextUri = CurrentUri;
                        CurrentUri = PreviousUri;
                        PreviousUri = null;
                    });
                    SkipPreviousCommand?.Execute(null);
                }
            }
            else
            {
                await Center.ResetOffsetAsync();
            }
        }

        private void Center_OnOffsetXChanged(object sender, double e)
        {
            if (Math.Abs(Center.OffsetX) > ActualWidth) return;
            Canvas.SetZIndex(Left, Center.OffsetX > 0 ? 1 : -1);
            Canvas.SetZIndex(Right, Center.OffsetX < 0 ? 1 : -1);
            Left.Opacity = Center.OffsetX > 0 ? 1 : 0;
            Right.Opacity = Center.OffsetX < 0 ? 1 : 0;

            ((CompositeTransform)Right.RenderTransform).TranslateX = SideImageFactor * ActualWidth + e * SideImageFactor;
            ((CompositeTransform)Left.RenderTransform).TranslateX = -SideImageFactor * ActualWidth + e * SideImageFactor;
        }

        enum Direction
        {
            Left,
            Right,
        }

        private void MoveCenterToEdge(Direction direction)
        {
            var centerCoverAnim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.2),
                From = 0,
                To = direction == Direction.Left ? -ActualWidth : ActualWidth,
                EasingFunction = new PowerEase(),
            };

            Storyboard.SetTarget(centerCoverAnim, Center);
            Storyboard.SetTargetProperty(centerCoverAnim, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");

            var sb = new Storyboard();
            sb.Children.Add(centerCoverAnim);
            sb.Begin();

            if (direction == Direction.Left)
                MoveRightToCenter.Begin();
            else
                MoveLeftToCenter.Begin();
        }

        private void MoveRightToCenter_OnCompleted(object sender, object e)
        {
            ResetCenterImage(Direction.Right);
            _lastToNextCallback?.Invoke();
            _lastToNextCallback = null;
        }

        private void MoveLeftToCenter_OnCompleted(object sender, object e)
        {
            ResetCenterImage(Direction.Left);
            _lastToPrevisouCallback?.Invoke();
            _lastToPrevisouCallback = null;
        }

        private void ResetCenterImage(Direction toDirection)
        {
            Center.Source = toDirection == Direction.Left ? Left.Source : Right.Source;

            Center.Reset();
            ((CompositeTransform)Center.RenderTransform).TranslateX = 0;

            ResetBackImagesLayout();
        }

        private Action _lastToNextCallback;

        public void SkipToNext(Action animationCallback)
        {
            _lastToNextCallback = animationCallback;

            Canvas.SetZIndex(Right, 1);
            Canvas.SetZIndex(Left, -1);
            MoveCenterToEdge(Direction.Left);
        }

        private Action _lastToPrevisouCallback;

        public void SkipToPrevious(Action animationCallback)
        {
            _lastToPrevisouCallback = animationCallback;

            Canvas.SetZIndex(Right, -1);
            Canvas.SetZIndex(Left, 1);
            MoveCenterToEdge(Direction.Right);
        }
    }
}

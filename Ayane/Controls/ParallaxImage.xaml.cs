using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Controls
{
    public sealed partial class ParallaxImage : UserControl
    {
        public ParallaxImage()
        {
            InitializeComponent();
            InitManipulationTransforms();

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;

            Container.ManipulationStarted += (sender, args) => OnInteractionStarted(args.Position);
            Container.ManipulationDelta += Container_ManipulationDelta;
            Container.ManipulationCompleted += (sender, args) => OnInteractionCompleted();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Container.Clip = new RectangleGeometry { Rect = new Rect { Width = ActualWidth, Height = ActualHeight } };
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Container.Clip = new RectangleGeometry { Rect = new Rect { Width = ActualWidth, Height = ActualHeight } };
        }

        public event EventHandler<double> OffsetXChanged;

        public event EventHandler InteractionCompleted;

        public event EventHandler<Point> InteractionStarted;

        public double DecodePixelWidth { get { return Image.DecodePixelWidth; } set { Image.DecodePixelWidth = (int)value; } }

        public double DecodePixelHeight { get { return Image.DecodePixelHeight; } set { Image.DecodePixelHeight = (int)value; } }

        public Uri UriSource { get { return GetValue(UriSourceProperty) as Uri; } set { SetValue(UriSourceProperty, value); } }

        public double Factor { get; set; } = 1d;

        public bool IsReverse { get; set; } = false;

        public ImageSource Source { get { return Image.Source; } set { Image.Source = value; } }

        public bool UseAnimation { get { return Image.UseAnimation; } set { Image.UseAnimation = value; } }

        public double InitTranslationX { set { SetValue(InitTranslationXProperty, value); } }

        public double InitContentTranslationX { set { UpdateContentTranslationDelta(value); } }

        public double OffsetX => containerTransformGroup.Value.OffsetX;

        public double ContentOffsetX => contentTransformGroup.Value.OffsetX;

        public static DependencyProperty InitTranslationXProperty = DependencyProperty.Register(nameof(InitTranslationX), typeof(double), typeof(ParallaxImage), new PropertyMetadata(0, (o, args) => ((ParallaxImage)o).UpdateTranslationDelta((double)args.NewValue)));

        public static DependencyProperty UriSourceProperty = DependencyProperty.Register(nameof(UriSource), typeof(Uri), typeof(ParallaxImage), new PropertyMetadata(null, (o, args) => ((ParallaxImage)o).Image.UriSource = args.NewValue as Uri));

        private void OnInteractionCompleted()
        {
            InteractionCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void OnInteractionStarted(Point e)
        {
            InteractionStarted?.Invoke(this, e);
        }

        private void OnOffsetXChanged(double e)
        {
            OffsetXChanged?.Invoke(this, e);
            Debug.WriteLine($"{Name} OffsetX: {e}");
        }
    }

    partial class ParallaxImage
    {
        private TransformGroup containerTransformGroup;
        private MatrixTransform containerPreviousTransform;
        private CompositeTransform containerDeltaTransform;

        private TransformGroup contentTransformGroup;
        private MatrixTransform contentPreviousTransform;
        private CompositeTransform contentDeltaTransform;

        private void InitManipulationTransforms()
        {
            containerTransformGroup = new TransformGroup();
            containerPreviousTransform = new MatrixTransform() { Matrix = Matrix.Identity };
            containerDeltaTransform = new CompositeTransform();
            containerTransformGroup.Children.Add(containerPreviousTransform);
            containerTransformGroup.Children.Add(containerDeltaTransform);
            Container.RenderTransform = containerTransformGroup;

            contentTransformGroup = new TransformGroup();
            contentPreviousTransform = new MatrixTransform() { Matrix = Matrix.Identity };
            contentDeltaTransform = new CompositeTransform();
            contentTransformGroup.Children.Add(contentPreviousTransform);
            contentTransformGroup.Children.Add(contentDeltaTransform);
            Image.RenderTransform = contentTransformGroup;
        }

        private void Container_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            UpdateTranslationDelta(e.Delta.Translation.X);
        }

        public void UpdateTranslationDelta(double deltaTx)
        {
            UpdateContainerTranslation(deltaTx);
            UpdateContentTranslationDelta(deltaTx);
            OnOffsetXChanged(containerTransformGroup.Value.OffsetX);
        }

        private void UpdateContainerTranslation(double deltaTx)
        {
            containerPreviousTransform.Matrix = containerTransformGroup.Value;
            containerDeltaTransform.TranslateX = deltaTx;
        }

        public void UpdateContentTranslationDelta(double deltaTx)
        {
            contentPreviousTransform.Matrix = contentTransformGroup.Value;
            contentDeltaTransform.TranslateX = deltaTx * (IsReverse ? -Factor : Factor);
        }

        public async Task ResetOffsetAsync()
        {
            var interval = TimeSpan.FromMilliseconds(1);
            var step = Math.Abs(OffsetX / 20);

            while (OffsetX > 0)
            {
                UpdateTranslationDelta(-step);
                await Task.Delay(interval);
            }

            while (OffsetX < 0)
            {
                UpdateTranslationDelta(step);
                await Task.Delay(interval);
            }

            if (Math.Abs(OffsetX) > 0.001) UpdateTranslationDelta(-OffsetX);
        }

        public void Reset()
        {
            Container.RenderTransform = null;
            Image.RenderTransform = null;

            InitManipulationTransforms();
        }

        public async Task SetUriSourceAsync(Uri uri)
        {
            await Image.SetUriSourceAsync(uri);
        }
    }
}

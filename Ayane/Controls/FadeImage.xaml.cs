using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

namespace Ayane.Controls
{
    public sealed partial class FadeImage : UserControl
    {
        public FadeImage()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler ImageProcessed;

        public double DecodePixelWidth { get; set; } = 200;
        public double DecodePixelHeight { get; set; } = 200;
        public bool UseAnimation { get; set; } = true;

        public Uri UriSource { get { return GetValue(UriSourceDependencyProperty) as Uri; } set { SetValue(UriSourceDependencyProperty, value); } }
        public static DependencyProperty UriSourceDependencyProperty = DependencyProperty.Register(nameof(UriSource), typeof(Uri), typeof(FadeImage), new PropertyMetadata(null, OnUriSourceChanged));

        private Uri _processingUri;
        private static async void OnUriSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == args.OldValue) return;
            var me = (FadeImage)obj;
            if (me._processingUri != null) return;

            var uri = args.NewValue as Uri;

            if (me.UseAnimation) me.FadeOut.Begin();
            if (uri == null)
            {
                me.RootGrid.Background = me.Background;
                me.Image.Source = null;
                me.OnImageProcessed(null);
                return;
            }

            me._processingUri = uri;

            LoadImage:

            await me.SetUriSourceAsync(me._processingUri);

            if (me._processingUri != me.UriSource)
            {
                me._processingUri = me.UriSource;
                goto LoadImage;
            }

            me._processingUri = null;

            me.OnImageProcessed(null);
        }

        private void FadeIn_OnCompleted(object sender, object e)
        {
            _processingUri = null;
            RootGrid.Background = new ImageBrush { ImageSource = Image.Source };
        }

        private void OnImageProcessed(RoutedEventArgs e)
        {
            ImageProcessed?.Invoke(this, e);
        }

        public async Task SetUriSourceAsync(Uri uri)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(uri.OriginalString);

                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmap = new BitmapImage();

                    if (DecodePixelWidth > 0) bitmap.DecodePixelWidth = (int)DecodePixelWidth;
                    if (DecodePixelHeight > 0) bitmap.DecodePixelHeight = (int)DecodePixelHeight;

                    await bitmap.SetSourceAsync(stream);
                    Image.Source = bitmap;
                }

                if (UseAnimation) FadeIn.Begin();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                if (UseAnimation) FadeOut.Begin();
                Image.Source = null;
            }
        }

        public ImageSource Source { get { return Image.Source; } set { Image.Source = value; } }
    }

}

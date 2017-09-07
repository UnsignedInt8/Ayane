using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Ayane.Common;
using Ayane.FrameworkEx;
using Ayane.Widgets;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UIDebuggingPage : Page
    {
        public UIDebuggingPage()
        {
            this.InitializeComponent();

            ParallaxImage.UriSource = new Uri("C:\\Users\\UnsignedInt8\\AppData\\Local\\Packages\\48c52126-72bd-4fac-9b64-e71bd42f74d6_mr1a29a7370sr\\LocalState\\Covers\\0767a669932e5bd17f679a36d82af042");
            ParallaxImage.DoubleTapped += (sender, args) => ParallaxImage.Reset();
            ParallaxCover.PreviousUri = new Uri("C:\\Users\\UnsignedInt8\\AppData\\Local\\Packages\\48c52126-72bd-4fac-9b64-e71bd42f74d6_mr1a29a7370sr\\LocalState\\Covers\\0767a669932e5bd17f679a36d82af042");
            ParallaxCover.CurrentUri = new Uri("C:\\Users\\UnsignedInt8\\AppData\\Local\\Packages\\48c52126-72bd-4fac-9b64-e71bd42f74d6_mr1a29a7370sr\\LocalState\\Covers\\f52a6c9b0aa0487ba949503a6d7e14c7");
            ParallaxCover.NextUri = new Uri("C:\\Users\\UnsignedInt8\\AppData\\Local\\Packages\\48c52126-72bd-4fac-9b64-e71bd42f74d6_mr1a29a7370sr\\LocalState\\Covers\\3403889257ae690b4ef3581c4d89fcaa");
        }

        private bool _isPressed;
        private Point _startPoint;
        private void ParallaxImageOnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            _isPressed = true;
            _startPoint = args.GetCurrentPoint(ParallaxImage).Position;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (_isPressed)
                ParallaxImage.InitTranslationX = args.GetCurrentPoint(ParallaxImage).Position.X - _startPoint.X;
        }

        private void UIDebuggingPage_Loaded(object sender, RoutedEventArgs e)
        {
            var bitmap = new BitmapImage(ImageUri);
            Cover.Source = bitmap;
        }

        public Uri ImageUri => new Uri("C:\\Users\\UnsignedInt8\\AppData\\Local\\Packages\\48c52126-72bd-4fac-9b64-e71bd42f74d6_mr1a29a7370sr\\LocalState\\Covers\\0767a669932e5bd17f679a36d82af042");
    }
}

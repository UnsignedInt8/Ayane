using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Ayane.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstLaunchPage : Page
    {
        public FirstLaunchPage()
        {
            InitializeComponent();
            Loaded += FirstLaunchPage_Loaded;
        }

        private void FirstLaunchPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FirstLaunchPage_Loaded;
            App.SetTitleBarInDarkBackground();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            WelcomeAnimation.Begin();
        }

        private void WelcomeAnimation_OnCompleted(object sender, object e)
        {
            TransitionAnimation.Begin();
        }

        private void TransitionAnimation_OnCompleted(object sender, object e)
        {
            Frame.Navigate(typeof(Pages.CreatePlaylistPage), CommonKeys.ClearBackStack);
            App.ResetTitleBarToAccentColor();
        }
    }
}

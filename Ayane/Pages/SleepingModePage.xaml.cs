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
using Windows.UI.Xaml.Navigation;
using Ayane.FrameworkEx;
using Ayane.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SleepingModePage : Page
    {
        public SleepingModePage()
        {
            InitializeComponent();
            TimeContainer.ApplyOffsetAnimation();
            ViewModel = DataContext as SleepingModeViewModel;
        }

        SleepingModeViewModel ViewModel { get; set; }

        private void SleepingMode_OnClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }
}

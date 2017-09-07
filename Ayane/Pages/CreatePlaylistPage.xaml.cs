using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
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
using Windows.UI.Xaml.Navigation;
using Ayane.Common;
using Ayane.Models;
using Ayane.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePlaylistPage : Page
    {
        public CreatePlaylistPage()
        {
            InitializeComponent();
            DragArea.Drop += DragArea_OnDrop;
            DragArea.DragOver += DragArea_OnDragOver;

            ViewModel = DataContext as NewPlaylistViewModel;
            if (ViewModel == null) return;
            ViewModel.Created += ViewModel_Created;
        }

        NewPlaylistViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (CommonKeys.ClearBackStack.Equals(e.Parameter))
            {
                Frame.BackStack.Clear();
                TransitionToAnimation.Begin();
            }

            if (Frame.BackStackDepth == 0)
            {
                RootGrid.Padding = new Thickness(16, 32, 16, 12);
            }
        }

        private void DragArea_OnDragOver(object sender, DragEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(DragOverState), true);
        }

        private void DragArea_OnDragLeave(object sender, DragEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(DragNormalState), true);
        }

        private void DragArea_OnDropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            VisualStateManager.GoToState(this, nameof(DragNormalState), true);
        }

        private void DragArea_OnDrop(object sender, DragEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(DragNormalState), true);
        }

        private void ViewModel_Created(object sender, NewPlaylistViewModel.CreatePlaylistEventArgs e)
        {
            ViewModel.Created -= ViewModel_Created;

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                return;
            }

            Frame.Navigate(typeof(Pages.MainPage));
        }
    }
}

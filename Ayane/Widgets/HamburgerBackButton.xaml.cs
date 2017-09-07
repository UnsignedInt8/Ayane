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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Widgets
{
    public sealed partial class HamburgerBackButton : UserControl
    {
        public HamburgerBackButton()
        {
            InitializeComponent();
            DataContext = this;

            PointerEntered += (sender, args) => VisualStateManager.GoToState(this, nameof(PointerOverState), true);
            PointerExited += (sender, args) => VisualStateManager.GoToState(this, nameof(NormalState), true);
            PointerPressed += (sender, args) => VisualStateManager.GoToState(this, nameof(PressedState), true);
            PointerReleased += (sender, args) => VisualStateManager.GoToState(this, nameof(PointerOverState), true);
            PointerCanceled += (sender, args) => VisualStateManager.GoToState(this, nameof(NormalState), true);
            PointerCaptureLost += (sender, args) => VisualStateManager.GoToState(this, nameof(NormalState), true);

            Tapped += HamburgBackButton_Tapped;
        }

        private void HamburgBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_isMenuShow)
            {
                OnMenuClicked();
            }
            else
            {
                OnBackClicked();
            }
        }

        private bool _isMenuShow = true;

        public event EventHandler MenuClicked;
        public event EventHandler BackClicked;

        public void ShowBack()
        {
            _isMenuShow = false;
            ToBackAnimation.Begin();
        }

        public void ShowMenu()
        {
            _isMenuShow = true;
            ToMenuAnimation.Begin();
        }

        private void OnMenuClicked()
        {
            MenuClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke(this, EventArgs.Empty);
        }

        public new Brush Foreground { get { return MenuIcon.Foreground; } set { MenuIcon.Foreground = BackIcon.Foreground = value; } }
    }
}

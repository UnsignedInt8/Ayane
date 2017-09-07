using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
    public sealed partial class PopupContainer : UserControl
    {
        public PopupContainer()
        {
            InitializeComponent();
            if (DesignMode.DesignModeEnabled) return;

            PopupRoot.Width = LayoutRoot.Width = Window.Current.Bounds.Width;
            PopupRoot.Height = LayoutRoot.Height = Window.Current.Bounds.Height;
            PopupRoot.Closed += Closed;
            PopupRoot.Opened += Opened;

            Window.Current.SizeChanged += WindowOnSizeChanged;
        }

        private void WindowOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            PopupRoot.Width = LayoutRoot.Width = Window.Current.Bounds.Width;
            PopupRoot.Height = LayoutRoot.Height = Window.Current.Bounds.Height;
        }

        public event EventHandler<object> Opened;
        public event EventHandler<object> Closed;

        public void Open()
        {
            PopupRoot.IsOpen = true;
        }

        public void Close()
        {
            PopupRoot.IsOpen = false;
            Window.Current.SizeChanged -= WindowOnSizeChanged;
        }

        public UIElement LayoutContent { get; set; }
        public static DependencyProperty LayoutContentDependencyProperty = DependencyProperty.Register(nameof(LayoutContent), typeof(UIElement), typeof(PopupContainer), null);

        public Color BackgroundColor => ((SolidColorBrush)Background)?.Color ?? Colors.Transparent;
    }
}

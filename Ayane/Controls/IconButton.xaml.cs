using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Ayane.Controls
{
    public sealed partial class IconButton : UserControl
    {
        public IconButton()
        {
            InitializeComponent();

            PointerEntered += (sender, args) => VisualStateManager.GoToState(this, "PointerOver", true);
            PointerExited += (sender, args) => VisualStateManager.GoToState(this, "Normal", true);
            PointerPressed += (sender, args) => VisualStateManager.GoToState(this, "Pressed", true);
            PointerReleased += (sender, args) => VisualStateManager.GoToState(this, "PointerOver", true);
            PointerCanceled += (sender, args) => VisualStateManager.GoToState(this, "Normal", true);
            PointerCaptureLost += (sender, args) => VisualStateManager.GoToState(this, "Normal", true);
            Tapped += (sender, args) => OnClicked();
            DataContext = this;
            
        }

        public event EventHandler Click;

        public Geometry PathData { get { return GetValue(PathDataDependencyProperty) as Geometry; } set { SetValue(PathDataDependencyProperty, value); } }
        public static DependencyProperty PathDataDependencyProperty = DependencyProperty.Register(nameof(PathData), typeof(Geometry), typeof(IconButton), null);

        public double IconHeight { get { return Icon.Height; } set { Icon.Height = value; } }
        public double IconWidth { get { return Icon.Width; } set { Icon.Width = value; } }

        private void OnClicked()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}

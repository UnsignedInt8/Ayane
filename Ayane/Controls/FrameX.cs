using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Ayane.Controls
{
    class FrameX : Frame
    {
        private Storyboard _showToastAnimation;
        private TextBlock _messageTextBlock;
        public Grid RootGrid { get; private set; }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RootGrid = (Grid)GetTemplateChild("RootContainer");
            _showToastAnimation = (Storyboard)RootGrid.Resources["ShowToastMessageAnimation"];
            _messageTextBlock = (TextBlock)GetTemplateChild("ToastTextBlock");
        }

        public void ShowToastMessage(string message)
        {
            _messageTextBlock.Text = message;
            _showToastAnimation?.Begin();
        }

        public void Attach(FrameworkElement element)
        {
            if (RootGrid.Children.Contains(element)) return;
            Grid.SetRow(element, 0);
            Grid.SetColumn(element, 0);
            Grid.SetRowSpan(element, 999);
            Grid.SetColumnSpan(element, 999);
            RootGrid.Children.Add(element);
        }

        public void Deattach(UIElement element)
        {
            RootGrid.Children.Remove(element);
        }
    }
}

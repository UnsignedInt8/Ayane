using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;

namespace Ayane.Themes
{
    class TitleBarBehavior : DependencyObject
    {
        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            var newTitleBar = associatedObject as UIElement;
            if (newTitleBar == null) throw new ArgumentException("TitleBarBehavior can be attached only to UIElement");

            Window.Current.SetTitleBar(newTitleBar);
        }

        public void Detach() { }

        public bool IsChromeless
        {
            get { return (bool)GetValue(IsChromelessProperty); }
            set { SetValue(IsChromelessProperty, value); }
        }

        public static readonly DependencyProperty IsChromelessProperty = DependencyProperty.Register("IsChromeless", typeof(bool), typeof(TitleBarBehavior), new PropertyMetadata(false, OnIsChromelessChanged));

        private static void OnIsChromelessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar
                .ExtendViewIntoTitleBar = (bool)e.NewValue;
        }
    }
}

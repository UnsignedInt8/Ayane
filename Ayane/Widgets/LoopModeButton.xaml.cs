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
    public sealed partial class LoopModeButton : UserControl
    {
        public LoopModeButton()
        {
            InitializeComponent();
        }
        
        public static readonly DependencyProperty IsRepeatOneProperty = DependencyProperty.Register("IsRepeatOne", typeof(bool), typeof(LoopModeButton), new PropertyMetadata(false, IsRepeatOneChangedCallback));
        public static readonly DependencyProperty IsRepeatAllProperty = DependencyProperty.Register("IsRepeatAll", typeof(bool), typeof(LoopModeButton), new PropertyMetadata(false, IsRepeatAllChangedCallback));

        private static void IsRepeatAllChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var me = (LoopModeButton)d;
            var value = (bool)args.NewValue;
            me.RepeatAllButton.IsChecked = value;

            me.RepeatOneButton.Visibility = Visibility.Visible;
            me.RepeatAllButton.Visibility = Visibility.Visible;
            if (value)
            {
                me.RepeatOneButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                me.RepeatAllButton.Visibility = Visibility.Collapsed;
            }
        }

        private static void IsRepeatOneChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var me = (LoopModeButton)d;
            var value = (bool)args.NewValue;
            me.RepeatOneButton.IsChecked = value;

            me.RepeatOneButton.Visibility = Visibility.Visible;
            me.RepeatAllButton.Visibility = Visibility.Visible;

            if (value)
            {
                me.RepeatAllButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                me.RepeatOneButton.Visibility = Visibility.Collapsed;
            }
        }
        
        public bool IsRepeatOne
        {
            get { return (bool)GetValue(IsRepeatOneProperty); }
            set { SetValue(IsRepeatOneProperty, value); }
        }

        public bool IsRepeatAll
        {
            get { return (bool)GetValue(IsRepeatAllProperty); }
            set { SetValue(IsRepeatAllProperty, value); }
        }

        private void RepeatAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RepeatAllButton.IsChecked != null) IsRepeatAll = RepeatAllButton.IsChecked.Value;
        }

        private void RepeatOneButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RepeatOneButton.IsChecked != null) IsRepeatOne = RepeatOneButton.IsChecked.Value;
        }
        
        private void RepeatOneButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            RepeatOneButton.Visibility = Visibility.Collapsed;
            RepeatAllButton.Visibility = Visibility.Visible;
        }

        private void RepeatAllButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            IsRepeatOne = true;
        }
    }
}

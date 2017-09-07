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
using Ayane.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Widgets
{
    sealed partial class ActiveSongIndicator : UserControl
    {
        public ActiveSongIndicator()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            HideAnimation.Begin();
            Refresh();
        }

        private void Refresh()
        {
            if (CurrentSong?.Equals(TargetSong) ?? false)
            {
                ShowAnimation.Begin();
            }
            else
            {
                HideAnimation.Begin();
            }
        }

        public Song CurrentSong { get; set; }
        public DependencyProperty SongProperty = DependencyProperty.Register(nameof(CurrentSong), typeof(Song), typeof(ActiveSongIndicator), new PropertyMetadata(null, SongPropertyChanged));

        public Song TargetSong { get { return GetValue(TargetSongProperty) as Song; } set { SetValue(TargetSongProperty, value); } }
        public DependencyProperty TargetSongProperty = DependencyProperty.Register(nameof(TargetSong), typeof(Song), typeof(ActiveSongIndicator), new PropertyMetadata(null, SongPropertyChanged));

        private static void SongPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var me = (ActiveSongIndicator)obj;
            me.Refresh();
        }

    }
}

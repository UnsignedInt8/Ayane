using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Ayane.Common;
using Ayane.FrameworkEx;
using Ayane.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Widgets
{
    sealed partial class NowPlayingSpotlight : UserControl
    {
        public NowPlayingSpotlight()
        {
            InitializeComponent();
        }

        public MediaPlaybackState State { get { return (MediaPlaybackState)GetValue(MeidaPlaybackStateDependencyProperty); } set { SetValue(MeidaPlaybackStateDependencyProperty, value); } }

        public Song Song
        {
            get { return GetValue(SongDependencyProperty) as Song; }
            set { SetValue(SongDependencyProperty, value); }
        }

        public TimeSpan Position { get { return (TimeSpan)GetValue(PositionDependencyProperty); } set { SetValue(PositionDependencyProperty, value); } }

        public ICommand PlayCommand { get { return GetValue(PlayCommandProperty) as ICommand; } set { SetValue(PlayCommandProperty, value); } }

        public ICommand PauseCommand { get { return GetValue(PauseCommandProperty) as ICommand; } set { SetValue(PauseCommandProperty, value); } }

        private TimeSpan Duration { get; set; }

        private bool HasCover { get; set; }

        private bool IsHidden { get; set; }

        private bool AlwaysHidden { get; set; }

        public bool IsPlaying { get { return (bool)GetValue(IsPlayingDependencyProperty); } set { SetValue(IsPlayingDependencyProperty, value); } }

        public static DependencyProperty IsPlayingDependencyProperty = DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(NowPlayingSpotlight), new PropertyMetadata(false, (o, args) => ((NowPlayingSpotlight)o).PlayButton.IsPlaying = (bool)args.NewValue));

        public static DependencyProperty MeidaPlaybackStateDependencyProperty = DependencyProperty.Register(nameof(State), typeof(MediaPlaybackState), typeof(NowPlayingSpotlight), new PropertyMetadata(MediaPlaybackState.None, OnPlaybackStateChanged));

        public static DependencyProperty SongDependencyProperty = DependencyProperty.Register(nameof(Song), typeof(Song), typeof(NowPlayingSpotlight), new PropertyMetadata(null, OnSongChanged));

        public static DependencyProperty PositionDependencyProperty = DependencyProperty.Register(nameof(Position), typeof(TimeSpan), typeof(NowPlayingSpotlight), new PropertyMetadata(TimeSpan.Zero, OnPositionChanged));

        public static DependencyProperty PlayCommandProperty = DependencyProperty.Register(nameof(PlayCommand), typeof(ICommand), typeof(NowPlayingSpotlight), null);

        public static DependencyProperty PauseCommandProperty = DependencyProperty.Register(nameof(PauseCommand), typeof(ICommand), typeof(NowPlayingSpotlight), null);

        private static void OnPlaybackStateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var me = (NowPlayingSpotlight)dependencyObject;
            var state = (MediaPlaybackState)args.NewValue;

            switch (state)
            {
                case MediaPlaybackState.Playing:
                    me.CoversRotationAnimation.Begin();
                    if (me.HasCover) me.HidePlayButton.Begin();
                    break;
                default:
                    var angle = ((RotateTransform)me.CoversContainer.RenderTransform).Angle;

                    if (angle >= 180) me.ResetCoverRotationSecondHalfAnimation.Begin();
                    else me.ResetCoverRotationFirstHalfAnimation.Begin();
                    me.CoversRotationAnimation.Stop();
                    me.ShowPlayButton.Begin();
                    break;
            }
        }

        private static void OnPositionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var me = (NowPlayingSpotlight)dependencyObject;
            var position = (TimeSpan)args.NewValue;
            if (me.Duration.TotalMilliseconds < me.Position.TotalMilliseconds || me.Duration.TotalMilliseconds < 100)
            {
                me.ProgressBar.Percentage = 0;
                return;
            }

            me.ProgressBar.Percentage = position.TotalMilliseconds / me.Duration.TotalMilliseconds * 100;
        }

        private static async void OnSongChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var me = (NowPlayingSpotlight)dependencyObject;
            me.ProgressBar.Percentage = 0;

            var song = args.NewValue as Song;
            if (song == null)
            {
                me.Hide();
                me.ResetCovers();
                return;
            }

            if (me.IsHidden) me.Show();

            me.Duration = song.Duration;
            me.ProgressBar.Percentage = 0;

            if (string.IsNullOrEmpty(song.CoverFilePath))
            {
                me.ResetCovers();
                return;
            }

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(song.CoverFilePath);
                using (var stream = await file.OpenReadAsync())
                {
                    var bitmap = new BitmapImage { DecodePixelWidth = 300, DecodePixelHeight = 300 };
                    await bitmap.SetSourceAsync(stream);
                    me.Cover.Fill = new ImageBrush { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
                    me.ShowCoverAnimation.Begin();
                }

                me.HasCover = true;
                if (me.State == MediaPlaybackState.Playing) me.HidePlayButton.Begin();
            }
            catch (Exception)
            {
                me.ResetCovers();
            }
        }

        private void ResetCovers()
        {
            HasCover = false;
            BackCover.Fill = new SolidColorBrush(Colors.White);
            Cover.Fill = Foreground;
            ShowCoverAnimation.Begin();
            ShowPlayButton.Begin();
        }

        public void Show(bool force = false)
        {
            if (force) AlwaysHidden = false;

            if (AlwaysHidden) return;
            if (Song == null) return;
            if (IsHidden == false) return;

            ShowAnimation.Begin();
            IsHidden = false;
        }

        public void Hide()
        {
            HideAnimation.Begin();
            IsHidden = true;
        }

        public void Hide(bool alwaysHidden)
        {
            AlwaysHidden = alwaysHidden;
            Hide();
        }

        private void ShowCoverAnimation_OnCompleted(object sender, object e)
        {
            if (!HasCover) return;
            BackCover.Fill = Cover.Fill;
        }

        private void PlayButton_OnPlayClicked(object sender, EventArgs e)
        {
            PlayCommand?.Execute(null);
        }

        private void PlayButton_OnPauseClicked(object sender, EventArgs e)
        {
            PauseCommand?.Execute(null);
        }

        public UIElement XamlCover => Cover;
    }
}

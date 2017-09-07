using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
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
    public sealed partial class PlayButton : UserControl
    {
        private bool _isPlayIconShow = true;

        public PlayButton()
        {
            InitializeComponent();
            DataContext = this;
            PointerEntered += (sender, args) => VisualStateManager.GoToState(this, nameof(PointerOver), true);
            PointerExited += (sender, args) => VisualStateManager.GoToState(this, nameof(Normal), true);
            PointerPressed += (sender, args) => VisualStateManager.GoToState(this, nameof(Pressed), true);
            PointerReleased += (sender, args) => VisualStateManager.GoToState(this, nameof(PointerOver), true);
            PointerCanceled += (sender, args) => VisualStateManager.GoToState(this, nameof(Normal), true);
            PointerCaptureLost += (sender, args) => VisualStateManager.GoToState(this, nameof(Normal), true);

            Tapped += PlayButton_Tapped;
        }

        private void PlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_isPlayIconShow)
            {
                OnPlayClicked();
            }
            else
            {
                OnPauseClicked();
            }
        }

        public bool IsPlaying { get { return (bool)GetValue(IsPlayingDependencyProperty); } set { SetValue(IsPlayingDependencyProperty, value); } }
        public static DependencyProperty IsPlayingDependencyProperty = DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(PlayButton), new PropertyMetadata(false, (o, args) =>
        {
            var me = (PlayButton)o;
            if ((bool)args.NewValue)
            {
                me.ToPauseState.Begin();
                me._isPlayIconShow = false;
            }
            else
            {
                me.ToPlayState.Begin();
                me._isPlayIconShow = true;
            }
        }));

        public event EventHandler PlayClicked;
        public event EventHandler PauseClicked;

        private void OnPlayClicked()
        {
            PlayClicked?.Invoke(this, EventArgs.Empty);
            PlayCommand?.Execute(null);
        }

        private void OnPauseClicked()
        {
            PauseClicked?.Invoke(this, EventArgs.Empty);
            PauseCommand?.Execute(null);
        }

        public ICommand PlayCommand { get { return GetValue(PlayCommandProperty) as ICommand; } set { SetValue(PlayCommandProperty, value); } }
        public static DependencyProperty PlayCommandProperty = DependencyProperty.Register(nameof(PlayCommand), typeof(ICommand), typeof(PlayButton), null);

        public ICommand PauseCommand { get { return GetValue(PauseCommandProperty) as ICommand; } set { SetValue(PauseCommandProperty, value); } }
        public static DependencyProperty PauseCommandProperty = DependencyProperty.Register(nameof(PauseCommand), typeof(ICommand), typeof(PlayButton), null);
    }
}

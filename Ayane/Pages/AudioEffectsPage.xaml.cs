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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Ayane.FrameworkEx;
using Ayane.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ayane.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AudioEffectsPage : Page
    {
        public AudioEffectsPage()
        {
            InitializeComponent();

            foreach (var element in new UIElement[] { EchoSwitch, EchoContainer, LimiterSwitch, LimiterContainer, ReverbSwitch, ReverbContainer, EffectsNoteTextBlock })
            {
                element.ApplyOffsetAnimation(TimeSpan.Zero);
            }

            ViewModel = DataContext as AudioEffectsViewModel;
        }

        private AudioEffectsViewModel ViewModel { get; set; }

        private void Equalizer_OnToggled(object sender, RoutedEventArgs e)
        {
            (EqualizerSwitch.IsOn ? CreateOpenStoryboard(EqualizerContainer) : CreateCloseStoryboard(EqualizerContainer)).Begin();
        }

        private void EchoSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            (EchoSwitch.IsOn ? CreateOpenStoryboard(EchoContainer) : CreateCloseStoryboard(EchoContainer)).Begin();
        }

        private void LimiterSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            (LimiterSwitch.IsOn ? CreateOpenStoryboard(LimiterContainer) : CreateCloseStoryboard(LimiterContainer)).Begin();
        }
        
        private void ReverbSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            (ReverbSwitch.IsOn ? CreateOpenStoryboard(ReverbContainer) : CreateCloseStoryboard(ReverbContainer)).Begin();
        }

        private Storyboard CreateOpenStoryboard(UIElement target)
        {
            var objKeyframem = new DiscreteObjectKeyFrame
            {
                KeyTime = TimeSpan.Zero,
                Value = Visibility.Visible
            };

            var objAnim = new ObjectAnimationUsingKeyFrames();
            objAnim.KeyFrames.Add(objKeyframem);
            Storyboard.SetTarget(objAnim, target);
            Storyboard.SetTargetProperty(objAnim, nameof(Visibility));

            var opactiyAnim = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                To = 1,
                From = 0,
            };
            Storyboard.SetTarget(opactiyAnim, target);
            Storyboard.SetTargetProperty(opactiyAnim, nameof(Opacity));

            var sb = new Storyboard();
            sb.Children.Add(objAnim);
            sb.Children.Add(opactiyAnim);

            return sb;
        }

        private Storyboard CreateCloseStoryboard(UIElement target)
        {
            var objKeyframe = new DiscreteObjectKeyFrame()
            {
                KeyTime = TimeSpan.FromMilliseconds(201),
                Value = Visibility.Collapsed,
            };

            var objAnim = new ObjectAnimationUsingKeyFrames();
            objAnim.KeyFrames.Add(objKeyframe);
            Storyboard.SetTarget(objAnim, target);
            Storyboard.SetTargetProperty(objAnim, nameof(Visibility));

            var opacityAnim = new DoubleAnimation()
            {
                Duration = TimeSpan.FromMilliseconds(200),
                To = 0,
            };
            Storyboard.SetTarget(opacityAnim, target);
            Storyboard.SetTargetProperty(opacityAnim, nameof(Opacity));

            var sb = new Storyboard();
            sb.Children.Add(objAnim);
            sb.Children.Add(opacityAnim);
            return sb;
        }

    }
}

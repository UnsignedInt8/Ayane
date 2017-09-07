using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Controls
{
    public sealed partial class FlyTextBlock : UserControl
    {
        private double _clipHeight = -1;
        private static readonly Dictionary<TextLeapMode, Func<EasingFunctionBase>> EasingTable = new Dictionary<TextLeapMode, Func<EasingFunctionBase>>
        {
            {TextLeapMode.BackEase, () => new BackEase() },
            {TextLeapMode.BounceEase, () => new BounceEase() },
            {TextLeapMode.CircleEase, () => new CircleEase() },
            {TextLeapMode.CubicEase, ()=> new CubicEase() },
            {TextLeapMode.ElasticEase, ()=>new ElasticEase() },
            {TextLeapMode.ExponentialEase, () => new ExponentialEase() },
            {TextLeapMode.PowerEase, () => new PowerEase() },
            {TextLeapMode.QuadraticEase, ()=> new QuadraticEase() },
            {TextLeapMode.QuarticEase, () => new QuarticEase() },
            {TextLeapMode.QuinticEase, () => new QuinticEase() },
            {TextLeapMode.SineEase, () => new SineEase() },
        };

        public FlyTextBlock()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += (sender, args) =>
            {
                if (_clipHeight > 0) return;

                var tb = new TextBlock { Text = "1", FontSize = FontSize, FontFamily = FontFamily, OpticalMarginAlignment = OpticalMarginAlignment.TrimSideBearings, TextLineBounds = TextLineBounds.TrimToBaseline };
                tb.Measure(Size.Empty);
                _clipHeight = tb.ActualHeight;
                Boundary.Rect = new Rect(0, 0.25 * _clipHeight, 9999, _clipHeight);
                ((CompositeTransform)DownsideTextBlock.RenderTransform).TranslateY = -_clipHeight;
            };
        }

        public string Text { get { return (string)GetValue(TextDependencyProperty); } set { SetValue(TextDependencyProperty, value); } }
        public static DependencyProperty TextDependencyProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(FlyTextBlock), new PropertyMetadata(string.Empty, async (o, args) =>
        {
            if (args.NewValue == args.OldValue) return;
            var me = (FlyTextBlock)o;

            await Task.Delay(me.Delay);

            me.UpsideTextBlock.Text = args.OldValue.ToString();
            me.DownsideTextBlock.Text = args.NewValue?.ToString() ?? string.Empty;
            me.Animate(() => me.UpsideTextBlock.Text = string.Empty);
        }));

        public TextAlignment TextAlignment { get { return (TextAlignment)GetValue(TextAlignmentDependencyProperty); } set { SetValue(TextAlignmentDependencyProperty, value); } }
        public static DependencyProperty TextAlignmentDependencyProperty = DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(FlyTextBlock), new PropertyMetadata(TextAlignment.Left));

        public TimeSpan Delay { get { return (TimeSpan)GetValue(DelayDependencyProperty); } set { SetValue(DelayDependencyProperty, value); } }
        public static DependencyProperty DelayDependencyProperty = DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(FlyTextBlock), new PropertyMetadata(TimeSpan.Zero));

        public TextLeapMode Mode { get { return (TextLeapMode)GetValue(TextSlippingModeProperty); } set { SetValue(TextSlippingModeProperty, value); } }

        public static DependencyProperty TextSlippingModeProperty = DependencyProperty.Register(nameof(Mode), typeof(TextLeapMode), typeof(FlyTextBlock), new PropertyMetadata(TextLeapMode.SineEase, null));

        private void Animate(Action completed = null)
        {
            ((CompositeTransform)UpsideTextBlock.RenderTransform).TranslateY = 0;
            UpsideTextBlock.Opacity = 1;
            const string targetProperty = "(UIElement.RenderTransform).(CompositeTransform.TranslateY)";
            var duration = TimeSpan.FromSeconds(.4);
            var easing = EasingTable[Mode]();

            var upsideStartKeyframe = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.Zero,
                Value = 0
            };

            var upsideEndKeyframe = new EasingDoubleKeyFrame
            {
                KeyTime = duration,
                Value = -1 * _clipHeight,
                EasingFunction = easing
            };

            var upsideKeyframes = new DoubleAnimationUsingKeyFrames();
            upsideKeyframes.KeyFrames.Add(upsideStartKeyframe);
            upsideKeyframes.KeyFrames.Add(upsideEndKeyframe);
            Storyboard.SetTarget(upsideKeyframes, UpsideTextBlock);
            Storyboard.SetTargetProperty(upsideKeyframes, targetProperty);

            var upsideOpacity = new DoubleAnimation { To = 0, Duration = duration, };
            Storyboard.SetTarget(upsideOpacity, UpsideTextBlock);
            Storyboard.SetTargetProperty(upsideOpacity, "Opacity");
            var upside = new Storyboard();
            upside.Children.Add(upsideKeyframes);
            upside.Children.Add(upsideOpacity);
            upside.Begin();

            ((CompositeTransform)DownsideTextBlock.RenderTransform).TranslateY = 0;
            var downsideStartKeyframe = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.Zero,
                Value = _clipHeight,
            };

            var downsideEndKeyframe = new EasingDoubleKeyFrame
            {
                KeyTime = duration,
                Value = 0,
                EasingFunction = easing,
            };

            var downsideKeyframes = new DoubleAnimationUsingKeyFrames() { };
            downsideKeyframes.KeyFrames.Add(downsideStartKeyframe);
            downsideKeyframes.KeyFrames.Add(downsideEndKeyframe);
            Storyboard.SetTarget(downsideKeyframes, DownsideTextBlock);
            Storyboard.SetTargetProperty(downsideKeyframes, targetProperty);

            var downsideSb = new Storyboard();
            downsideSb.Children.Add(downsideKeyframes);
            downsideSb.Begin();
            downsideSb.Completed += (s, args) =>
            {
                completed?.Invoke();
            };
        }

        public enum TextLeapMode
        {
            BackEase,
            BounceEase,
            CircleEase,
            CubicEase,
            ElasticEase,
            ExponentialEase,
            PowerEase,
            QuadraticEase,
            QuarticEase,
            QuinticEase,
            SineEase
        }
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Ayane.FrameworkEx;
using Microsoft.Graphics.Canvas.Effects;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Controls
{
    public sealed partial class BlurGlass : UserControl
    {
        private SpriteVisual _blurVisual;

        public BlurGlass()
        {
            InitializeComponent();

            if (DesignMode.DesignModeEnabled) return;
            InitializeBlurVisual();
            SizeChanged += BlurGlass_SizeChanged;
        }

        private void BlurGlass_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _blurVisual.Size = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
            foreach (var line in new[] { LeftLine, TopLine, RightLine, BottomLine })
            {
                var shadowVisual = ElementCompositionPreview.GetElementChildVisual(line);
                if (shadowVisual == null) continue;
                shadowVisual.Size = new Vector2((float)line.ActualWidth, (float)line.ActualHeight);
            }
        }

        private void InitializeBlurVisual()
        {
            var c = this.GetVisual().Compositor;

            _blurVisual = _blurVisual ?? c.CreateSpriteVisual();
            _blurVisual.Brush = BuildBlurBrush(c, (float)(BlurOn ? BlurAmount : 0), MaskColor);

            ElementCompositionPreview.SetElementChildVisual(Glass, _blurVisual);
        }

        private CompositionEffectBrush BuildBlurBrush(Compositor c, float blurAmount, Color maskColor)
        {
            var blurDesc = new GaussianBlurEffect
            {
                Name = "GlassBlur",
                BlurAmount = blurAmount,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("Source")
            };

            var colorDesc = new ColorSourceEffect
            {
                Name = "GlassColor",
                Color = maskColor,
            };

            var blendEffectDesc = new BlendEffect
            {
                Mode = BlendEffectMode.Multiply,
                Background = blurDesc,
                Foreground = colorDesc,
            };

            var blurBrush = c.CreateEffectFactory(blendEffectDesc, new[] { "GlassBlur.BlurAmount", "GlassColor.Color" }).CreateBrush();
            blurBrush.SetSourceParameter("Source", c.CreateBackdropBrush());

            return blurBrush;
        }

        public bool BlurOn { get { return (bool)GetValue(BlurOnDependencyProperty); } set { SetValue(BlurOnDependencyProperty, value); } }
        public static DependencyProperty BlurOnDependencyProperty = DependencyProperty.Register(nameof(BlurOn), typeof(bool), typeof(BlurGlass), new PropertyMetadata(false, BlurEnableChanged));
        private static void BlurEnableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == args.OldValue) return;
            var me = (BlurGlass)obj;

            var a = me._blurVisual.Compositor.CreateScalarKeyFrameAnimation();
            a.Duration = TimeSpan.FromSeconds(1.2);
            a.InsertKeyFrame(1f, (float)(((bool)args.NewValue) ? me.BlurAmount : 0f));

            me._blurVisual.Brush.StartAnimation("GlassBlur.BlurAmount", a);
        }

        public double BlurAmount { get { return (double)GetValue(BlurAmountDependencyProperty); } set { SetValue(BlurAmountDependencyProperty, value); } }
        public static DependencyProperty BlurAmountDependencyProperty = DependencyProperty.Register(nameof(BlurAmount), typeof(double), typeof(BlurGlass), new PropertyMetadata(20d));

        public Color MaskColor { get { return (Color)GetValue(MaskColorDependencyProperty); } set { SetValue(MaskColorDependencyProperty, value); } }
        public static DependencyProperty MaskColorDependencyProperty = DependencyProperty.Register(nameof(MaskColor), typeof(Color), typeof(BlurGlass), new PropertyMetadata(Colors.Transparent, (o, args) =>
        {
            var newColor = (Color)(args.NewValue ?? Colors.Transparent);
            var me = (BlurGlass)o;
            var a = me._blurVisual.Compositor.CreateColorKeyFrameAnimation();
            a.Duration = TimeSpan.FromSeconds(1);
            a.InsertKeyFrame(1f, newColor);

            me._blurVisual.Brush.StartAnimation("GlassColor.Color", a);
        }));

        public double ShadowOpacity { get { return (double)GetValue(ShadowOpacityDependencyProperty); } set { SetValue(ShadowOpacityDependencyProperty, value); } }
        public static DependencyProperty ShadowOpacityDependencyProperty = DependencyProperty.Register(nameof(ShadowOpacity), typeof(double), typeof(BlurGlass), new PropertyMetadata(1d));

        public float ShadowBlurRadius { get { return (float)GetValue(ShadowBlurRadiusDependencyProperty); } set { SetValue(ShadowBlurRadiusDependencyProperty, value); } }
        public static DependencyProperty ShadowBlurRadiusDependencyProperty = DependencyProperty.Register(nameof(ShadowBlurRadius), typeof(float), typeof(BlurGlass), new PropertyMetadata(52f));

        public bool ShadowOn { get { return (bool)GetValue(ShadowOnDependencyProperty); } set { SetValue(ShadowOnDependencyProperty, value); } }
        public static DependencyProperty ShadowOnDependencyProperty = DependencyProperty.Register(nameof(ShadowOn), typeof(bool), typeof(BlurGlass), new PropertyMetadata(false, (o, args) =>
        {
            var me = (BlurGlass)o;
            if ((bool)args.NewValue)
            {
                var c = me.GetVisual().Compositor;
                if (ElementCompositionPreview.GetElementChildVisual(me.LeftLine) != null) return;

                var elements = new[] { me.LeftLine, me.TopLine, me.RightLine, me.BottomLine };
                foreach (var element in elements)
                {
                    var shadow = c.CreateDropShadow();
                    shadow.BlurRadius = me.ShadowBlurRadius;
                    shadow.Opacity = (float)me.ShadowOpacity;
                    shadow.Color = me.ShadowColor;

                    var shadowVisual = c.CreateSpriteVisual();
                    shadowVisual.Shadow = shadow;
                    shadowVisual.Size = element.GetVisual().Size;
                    ElementCompositionPreview.SetElementChildVisual(element, shadowVisual);
                }
            }
            else
            {
                foreach (var line in new[] { me.LeftLine, me.TopLine, me.RightLine, me.BottomLine })
                {
                    ElementCompositionPreview.SetElementChildVisual(line, null);
                }
            }
        }));

        public Color ShadowColor { get { return (Color)GetValue(ShadowColorDependencyProperty); } set { SetValue(ShadowColorDependencyProperty, value); } }
        public static DependencyProperty ShadowColorDependencyProperty = DependencyProperty.Register(nameof(ShadowColor), typeof(Color), typeof(BlurGlass), new PropertyMetadata(Colors.Black));
    }
}

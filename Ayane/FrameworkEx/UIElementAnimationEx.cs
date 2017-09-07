using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace Ayane.FrameworkEx
{
    static class UIElementAnimationEx
    {
        private static readonly Dictionary<string, ImplicitAnimationCollection> KeyframeAnimationsCache = new Dictionary<string, ImplicitAnimationCollection>();

        public static void ApplyOffsetAnimation(this UIElement element, TimeSpan? delayTime = null, TimeSpan? totalTime = null)
        {
            var visual = element.GetVisual();
            var c = visual.Compositor;
            delayTime = delayTime ?? TimeSpan.FromSeconds(0.15);
            totalTime = totalTime ?? TimeSpan.FromSeconds(0.5);

            var cacheKey = $"Keyframe_Offset_{delayTime}_{totalTime}";
            if (KeyframeAnimationsCache.ContainsKey(cacheKey))
            {
                visual.ImplicitAnimations = KeyframeAnimationsCache[cacheKey];
                return;
            }

            var offsetAnim = c.CreateVector3KeyFrameAnimation();
            offsetAnim.DelayTime = delayTime.Value;
            offsetAnim.Duration = totalTime.Value;
            offsetAnim.InsertExpressionKeyFrame(1f, "this.FinalValue");
            offsetAnim.Target = "Offset";
            var collection = c.CreateImplicitAnimationCollection();
            collection["Offset"] = offsetAnim;
            visual.ImplicitAnimations = collection;
            KeyframeAnimationsCache[cacheKey] = collection;
        }

        public static void ApplySizeAnimation(this UIElement element, TimeSpan? delay = null, TimeSpan? duration = null)
        {
            var visual = element.GetVisual();
            var c = visual.Compositor;
            delay = delay ?? TimeSpan.Zero;
            duration = duration ?? TimeSpan.FromSeconds(.5);

            var cacheKey = $"Keyframe_Size_{delay}_{duration}";
            if (KeyframeAnimationsCache.ContainsKey(cacheKey))
            {
                visual.ImplicitAnimations = KeyframeAnimationsCache[cacheKey];
                return;
            }

            var sizeAnim = c.CreateVector2KeyFrameAnimation();
            sizeAnim.DelayTime = delay.Value;
            sizeAnim.Duration = duration.Value;
            sizeAnim.InsertExpressionKeyFrame(1f, "this.FinalValue");
            sizeAnim.Target = "Size";
            var collection = c.CreateImplicitAnimationCollection();
            collection["Size"] = sizeAnim;
            visual.ImplicitAnimations = collection;
            KeyframeAnimationsCache[cacheKey] = collection;
        }
    }
}

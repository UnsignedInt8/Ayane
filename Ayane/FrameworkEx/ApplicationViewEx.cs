using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Ayane.FrameworkEx
{
    static class ApplicationViewEx
    {
        /// <summary>
        /// This method will resize window with animation, but it also will block the UI thread.
        /// </summary>
        /// <param name="appView"></param>
        /// <param name="toWidth"></param>
        /// <param name="toHeight"></param>
        public static void TryResizeWindowAnimation(this ApplicationView appView, double toWidth, double toHeight)
        {
            var curWdith = Window.Current.Bounds.Width;
            var curHeight = Window.Current.Bounds.Height;

            const int delta = 5;

            var widthPower = curWdith > toWidth ? -delta : delta;
            var heightPower = curHeight > toHeight ? -delta : delta;

            var toSmallerWidth = toWidth < curWdith;
            var toSmallerHeight = toHeight < curHeight;

            while (Math.Abs(curWdith - toWidth) > 0 || Math.Abs(curHeight - toHeight) > 0)
            {
                curWdith += widthPower;
                curWdith = toSmallerWidth ? Math.Max(curWdith, toWidth) : Math.Min(curWdith, toWidth);
                curHeight += heightPower;
                curHeight = toSmallerHeight ? Math.Max(curHeight, toHeight) : Math.Min(curHeight, toHeight);
                ApplicationView.GetForCurrentView().TryResizeView(new Size(curWdith, curHeight));
            }
        }

    }
}

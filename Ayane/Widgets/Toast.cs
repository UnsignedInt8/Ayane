using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Ayane.Controls;

namespace Ayane.Common
{
    class Toast
    {
        public static void ShowMessage(string message)
        {
            var rootFrame = Window.Current.Content as FrameX;
            rootFrame?.ShowToastMessage(message);
        }
    }
}

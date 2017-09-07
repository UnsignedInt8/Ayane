using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Display;

namespace Ayane.Common
{
    class DisplayRequestHelper
    {
        private static readonly DisplayRequest DisplayRequest = new DisplayRequest();

        public static void RequestActive()
        {
            DisplayRequest.RequestActive();
        }

        public static void RequestRelease()
        {
            DisplayRequest.RequestRelease();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Ayane.FrameworkEx
{
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}

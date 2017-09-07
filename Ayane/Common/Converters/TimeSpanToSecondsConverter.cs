using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Ayane.Common.Converters
{
    class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}

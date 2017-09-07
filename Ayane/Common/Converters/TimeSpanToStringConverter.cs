using System;
using Windows.UI.Xaml.Data;

namespace Ayane.Common.Converters
{
    class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var duration = (TimeSpan)value;
            return duration.ToString(duration.Hours > 0 ? "hh\\:mm\\:ss" : "mm\\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

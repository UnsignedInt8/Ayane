using System;
using System.Collections;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ayane.Common.Converters
{
    public class JumpListItemBackgroundConverter : IValueConverter
    {
        static readonly SolidColorBrush DisabledBrush = new SolidColorBrush(Color.FromArgb(0xAA, 0x1f, 0x1f, 0x1f));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var grouop = (IList)value;
            if (grouop.Count > 0) return new SolidColorBrush((Color)parameter);
            return DisabledBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

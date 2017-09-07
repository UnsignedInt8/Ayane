using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using Ayane.Models;

namespace Ayane.Common.Converters
{
    class ActiveSongToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var bindingSong = value as Song;
            var activeSong = parameter as Rectangle;
            if (bindingSong == null || activeSong == null) return Visibility.Collapsed;
            return bindingSong.Equals(activeSong) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

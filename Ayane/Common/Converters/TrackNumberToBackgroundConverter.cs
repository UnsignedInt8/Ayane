using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Ayane.Models;

namespace Ayane.Common.Converters
{
    class TrackNumberToBackgroundConverter : IValueConverter
    {
        private readonly SolidColorBrush _background = new SolidColorBrush(new Color { A = 28, R = 222, G = 222, B = 222});

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var song = value as Song;
            return song?.TrackNumber % 2 == 0 ? _background : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

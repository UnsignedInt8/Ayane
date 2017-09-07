using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Ayane.Common.Converters
{
    class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = (ListViewItem)value;
            var lv = ItemsControl.ItemsControlFromItemContainer(item) as ListViewBase;
            return lv?.IndexFromContainer(item) ?? 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

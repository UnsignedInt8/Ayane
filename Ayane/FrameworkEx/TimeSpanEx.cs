using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayane.FrameworkEx
{
    static class TimeSpanEx
    {
        public static string ToMusicalString(this TimeSpan time, string minutesFormat = @"mm\:ss")
        {
            var format = time.TotalHours > 1 ? @"h\:mm\:ss" : minutesFormat;
            return time.ToString(format);
        }
    }
}

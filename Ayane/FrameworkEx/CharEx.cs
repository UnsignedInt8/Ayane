using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayane.FrameworkEx
{
    static class CharEx
    {
        public static bool IsAlphabet(this Char c1)
        {
            return (c1 >= 'A' && c1 <= 'Z') || (c1 >= 'a' && c1 <= 'z');
        }
    }
}

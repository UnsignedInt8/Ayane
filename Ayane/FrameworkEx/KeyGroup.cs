using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayane.FrameworkEx
{
    class KeyGroup<TKey, TElemnt> : ObservableCollection<TElemnt>, IGrouping<TKey, TElemnt>
    {
        public TKey Key { get; set; }

        public KeyGroup() : base()
        {

        }

        public KeyGroup(IEnumerable<TElemnt> collection) : base(collection)
        {

        }
    }
}

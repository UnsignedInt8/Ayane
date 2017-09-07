using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Globalization.Collation;

namespace Ayane.FrameworkEx
{
    class AlphaKeyGroup<T> : ObservableCollection<T>
    {
        /// <summary>
        /// The delegate that is used to get the key information.
        /// </summary>
        /// <param name="item">An object of type T</param>
        /// <returns>The key value to use for this object</returns>
        public delegate string GetKeyDelegate(T item);

        /// <summary>
        /// The Key of this group.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="key">The key for this group.</param>
        public AlphaKeyGroup(string key)
        {
            Key = key.ToUpper();
        }

        public IEnumerable<T> Items => this;

        public static ObservableCollection<AlphaKeyGroup<T>> CreateGroups(IEnumerable<T> items, Func<T, string> getKey, bool ascending = true)
        {
            var alphaTable = new Dictionary<string, AlphaKeyGroup<T>>();
            LookupWorldCharacter(alphaTable, items, getKey);
            var result = alphaTable.Values.OrderBy(g => g.Key.Length);
            result = ascending ? result.OrderBy(g => g.Key) : result.OrderByDescending(g => g.Key);
            return new ObservableCollection<AlphaKeyGroup<T>>(result);
        }

        private static void LookupWorldCharacter<T>(Dictionary<string, AlphaKeyGroup<T>> table, IEnumerable<T> items, Func<T, string> getKey)
        {
            var chars = new CharacterGroupings();
            foreach (var c in chars.Where(ch => ch.Label.Length > 0).Select(ch => ch.Label[0].ToString().ToUpper()).Where(c => !table.ContainsKey(c) && c != "."))
            {
                table.Add(c, new AlphaKeyGroup<T>(c));
            }

            foreach (var item in items)
            {
                var key = getKey(item);

                var lookupChar = chars.Lookup(key).ToUpper();

                var index = lookupChar.Length > 1 ? (lookupChar.Any(CharEx.IsAlphabet) ? lookupChar.First(CharEx.IsAlphabet).ToString() : lookupChar) : lookupChar;

                if (table.ContainsKey(index))
                {
                    table[index].Add(item);
                }
                else
                {
                    table.Add(index, new AlphaKeyGroup<T>(index) { item });
                }
            }
        }

        public override string ToString()
        {
            return Key;
        }

        public class KeyComparer<T> : IEqualityComparer<AlphaKeyGroup<T>>
        {
            public bool Equals(AlphaKeyGroup<T> x, AlphaKeyGroup<T> y)
            {
                return x.Key == y.Key;
            }

            public int GetHashCode(AlphaKeyGroup<T> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}

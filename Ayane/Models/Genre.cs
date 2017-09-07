using System;
using System.Collections.Generic;
using System.Linq;

namespace Ayane.Models
{
    class Genre
    {
        public string Title { get; set; } = "Unknown";
        public IList<Song> Songs { get; set; }
        public int SongsCount => Songs?.Count ?? 0;
        public Uri CoverUri => Songs?.FirstOrDefault(s => s.CoverUri != null)?.CoverUri;

        public class TitleComparer : IEqualityComparer<string>
        {
            public bool Equals(Genre x, Genre y)
            {
                return x?.Title?.Equals(y?.Title, StringComparison.OrdinalIgnoreCase) ?? false;
            }

            public int GetHashCode(Genre obj)
            {
                return obj.Title.GetHashCode();
            }

            public bool Equals(string x, string y)
            {
                return x?.Trim().Equals(y?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase) ?? false;
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}

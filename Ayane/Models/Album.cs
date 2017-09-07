using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ayane.Models
{
    class Album
    {
        public string Title { get; set; } = "Unknown";
        public uint Year { get; set; }
        public IList<Song> Songs { get; set; }
        public Artist Artist { get; set; }

        private Uri _coverUri;
        public Uri CoverUri { get { return _coverUri ?? (_coverUri = Songs.FirstOrDefault(s => s.CoverUri != null)?.CoverUri); } }
        
        public override bool Equals(object obj)
        {
            var other = obj as Album;
            if (other == null) return false;

            return (Artist?.Equals(other.Artist) ?? false) && Title.Equals(other.Title);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Title}";
        }

        public class AlbumEqualityComparer : IEqualityComparer<Album>
        {
            public bool Equals(Album x, Album y)
            {
                return x?.Equals(y) ?? false;
            }

            public int GetHashCode(Album obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ayane.Models
{
    class Artist
    {
        public string Name { get; set; }
        public IList<Album> Albums { get; set; } = new List<Album>();
        public int SongsCount { get; set; }
        public bool HasAlbums => Albums.Any();

        private Uri _coverUri;
        public Uri CoverUri { get { return _coverUri ?? (_coverUri = Albums.FirstOrDefault(a => a.CoverUri != null)?.CoverUri); } }

        public override bool Equals(object obj)
        {
            return Name == (obj as Artist)?.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}

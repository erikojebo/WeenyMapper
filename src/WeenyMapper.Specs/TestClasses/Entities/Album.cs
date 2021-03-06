﻿using System.Collections.Generic;
using WeenyMapper.Extensions;
using System.Linq;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Album
    {
        public Album()
        {
            Tracks = new List<Track>();
        }

        public Album(string title) : this()
        {
            Title = title;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int? ArtistId { get; set; }

        public IList<Track> Tracks { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Album;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Title == other.Title &&
                   Tracks.ElementEquals(other.Tracks);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            var tracks = string.Join(", ", Tracks.Select(x => x.ToString()));

            return string.Format("Album Id: {0}, Title: {1}, Tracks: ({2})", Id, Title, tracks);
        }
    }
}
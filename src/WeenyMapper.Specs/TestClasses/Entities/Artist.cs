using System.Collections.Generic;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Artist
    {
        public Artist()
        {
            Albums = new List<Album>();
        }

        public Artist(string name) : this()
        {
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public IList<Album> Albums { get; set; }
    }
}
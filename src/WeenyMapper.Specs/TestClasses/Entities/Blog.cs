using System.Collections.Generic;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Blog
    {
        public Blog()
        {
            Posts = new List<BlogPost>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public IList<BlogPost> Posts { get; set; }
    }
}
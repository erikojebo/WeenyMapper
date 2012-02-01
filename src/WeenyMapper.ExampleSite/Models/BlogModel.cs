using System.Collections.Generic;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.ExampleSite.Models
{
    public class BlogModel
    {
        public Blog Blog { get; set; }
        public IList<BlogPost> BlogPosts { get; set; }
    }
}
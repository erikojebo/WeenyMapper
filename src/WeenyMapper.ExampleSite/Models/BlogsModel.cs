using System.Collections.Generic;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.ExampleSite.Models
{
    public class BlogsModel
    {
        public IList<Blog> Blogs { get; set; }
        public IList<BlogPost> Posts { get; set; }
    }
}
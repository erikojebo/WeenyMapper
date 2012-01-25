using System;
using System.Collections.Generic;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class BlogPost
    {
        public BlogPost()
        {
            Comments = new List<Comment>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }

        public User Author { get; set; }
        public IList<Comment> Comments { get; set; }
    }
}
using System;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
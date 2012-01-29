using System;
using System.Collections.Generic;
using WeenyMapper.Extensions;

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
        public Blog Blog { get; set; }
        public IList<Comment> Comments { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as BlogPost;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Title == other.Title &&
                   Content == other.Content &&
                   PublishDate == other.PublishDate &&
                   Author.NullSafeIdEquals(other.Author, x => x.Id) &&
                   Blog.NullSafeIdEquals(other.Blog, x => x.Id) &&
                   Comments.ElementEquals(other.Comments);
        }

        public override string ToString()
        {
            return string.Format("BlogPost Id: {0}, Title: {1}, Content: {2}, PublishDate: {3}", Id, Title, Content, PublishDate);
        }
    }
}
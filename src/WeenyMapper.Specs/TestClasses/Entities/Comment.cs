using System;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public BlogPost BlogPost { get; set; }
        public User User { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Comment;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Content == other.Content &&
                   PublishDate == other.PublishDate &&
                   Equals(BlogPost, other.BlogPost) &&
                   Equals(User, other.User);
        }

        public override string ToString()
        {
            return string.Format("Comment Id: {0}, Content: {1}, Publish date: {2}", Id, Content, PublishDate);
        }
    }
}
using System.Collections.Generic;
using WeenyMapper.Extensions;

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

        public void AddPost(BlogPost post)
        {
            Posts.Add(post);
            post.Blog = this;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Blog;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Name == other.Name &&
                   Posts.ElementEquals(other.Posts);
        }

        public override string ToString()
        {
            return string.Format("Blog Id: {0}, Name: {1}, Post count: {2}", Id, Name, Posts.Count);
        }
    }
}
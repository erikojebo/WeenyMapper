using System;
using System.Collections.Generic;
using WeenyMapper.Extensions;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class User
    {
        public User()
        {
            BlogPosts = new List<BlogPost>();
        }

        public User(string username, string password) : this()
        {
            Username = username;
            Password = password;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public IList<BlogPost> BlogPosts { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as User;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Username == other.Username &&
                   Password == other.Password &&
                   BlogPosts.ElementEquals(other.BlogPosts);
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Username: {1}, Password: {2}", Id, Username, Password);
        }

        public void AddBlogPost(BlogPost blogPost)
        {
            BlogPosts.Add(blogPost);
            blogPost.Author = this;
        }
    }
}
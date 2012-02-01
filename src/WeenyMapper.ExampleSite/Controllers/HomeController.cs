using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WeenyMapper.ExampleSite.Models;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.ExampleSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly Repository _repository;

        public HomeController()
        {
            _repository = new Repository();
        }

        public ActionResult Create()
        {
            CreateTestData();

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            // Doing top on a join query is not supported ATM since it makes no sense for 
            // one to many joins. However, it does make sense the other way around. So
            // maybe add support for it? Could be very confusing for those who do not
            // undestand the generated SQL though...

            // Since top is not supported for joins, simply use TOP to find the ids of the
            // posts we want and then fetch the posts and their relatives in a separate
            // query.

            var top10postIds = _repository.Find<BlogPost>()
                .Select(x => x.Id)
                .Top(10)
                .OrderByDescending(x => x.PublishDate)
                .ExecuteScalarList<int>();

            IList<BlogPost> posts = new List<BlogPost>();

            if (top10postIds.Any())
            {
                posts = _repository
                    .Find<BlogPost>()
                    .OrderByDescending(x => x.PublishDate)
                    .Where(x => top10postIds.Contains(x.Id))
                    .Join<User, BlogPost>(x => x.BlogPosts, x => x.Author)
                    .ExecuteList();
            }

            var blogs = _repository.Find<Blog>().ExecuteList();

            var model = new HomeModel
                {
                    Posts = posts,
                    Blogs = blogs
                };

            return View(model);
        }

        private void CreateTestData()
        {
            var codeBlog = new Blog { Name = "Code" };
            var bookBlog = new Blog { Name = "Books" };
            var screencastBlog = new Blog { Name = "Screencasts" };
            var podcastBlog = new Blog { Name = "Podcasts" };

            var steve = new User { Username = "Steve.Smith", Password = "password", Id = Guid.NewGuid() };
            var john = new User { Username = "John.Johnson", Password = "password", Id = Guid.NewGuid() };

            var users = new List<User> { steve, john };
            var posts = new List<BlogPost>();

            var random = new Random();

            for (int i = 0; i < 50; i++)
            {
                var post = CreatePost(i, codeBlog, users, random);
                posts.Add(post);
            }

            for (int i = 0; i < 21; i++)
            {
                var post = CreatePost(i, bookBlog, users, random);
                posts.Add(post);
            }

            for (int i = 0; i < 12; i++)
            {
                var post = CreatePost(i, screencastBlog, users, random);
                posts.Add(post);
            }

            for (int i = 0; i < 3; i++)
            {
                var post = CreatePost(i, podcastBlog, users, random);
                posts.Add(post);
            }

            var comments = posts.SelectMany(x => x.Comments);

            _repository.InsertCollection(users);
            _repository.Insert(codeBlog, bookBlog, screencastBlog, podcastBlog);
            _repository.InsertCollection(posts);
            _repository.InsertCollection(comments);
        }

        private BlogPost CreatePost(int index, Blog blog, IList<User> users, Random random)
        {
            var month = (index * 2) / 28 + 1;
            var day = (index * 2) % 28 + 1;
            var title = blog.Name + " Post " + index;
            var user = users[random.Next(users.Count)];

            var hour = random.Next(23);
            var minute = random.Next(59);
            var second = random.Next(59);

            var post = new BlogPost
                {
                    Title = title,
                    PublishDate = new DateTime(2011, month, day, hour, minute, second),
                    Author = user,
                    Blog = blog
                };

            post.Content = post.Title + " content";

            return post;
        }
    }
}
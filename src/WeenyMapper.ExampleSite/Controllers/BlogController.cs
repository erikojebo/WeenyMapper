using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WeenyMapper.ExampleSite.Models;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.ExampleSite.Controllers
{
    public class BlogController : Controller
    {
        private readonly Repository _repository;

        public BlogController()
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

            var top10PostIds = _repository.Find<BlogPost>()
                .Select(x => x.Id)
                .Top(10)
                .OrderByDescending(x => x.PublishDate)
                .ExecuteScalarList<int>();

            IList<BlogPost> posts = new List<BlogPost>();

            if (top10PostIds.Any())
            {
                posts = _repository
                    .Find<BlogPost>()
                    .OrderByDescending(x => x.PublishDate)
                    .Where(x => top10PostIds.Contains(x.Id))
                    .Join<User, BlogPost>(x => x.BlogPosts, x => x.Author)
                    .ExecuteList();
            }

            var blogs = _repository.Find<Blog>().ExecuteList();

            var model = new BlogsModel
                {
                    Posts = posts,
                    Blogs = blogs
                };

            return View(model);
        }

        public ActionResult Blog(int id, int page = 0)
        {
            var blog = _repository.Find<Blog>().Where(x => x.Id == id).Execute();

            var postIds = _repository.Find<BlogPost>()
                .Select(x => x.Id)
                .Where(x => x.Blog.Id == id)
                .Page(page, 5)
                .OrderByDescending(x => x.PublishDate)
                .ExecuteScalarList<int>();

            var model = new BlogModel
                {
                    Months = GetMonthsWithPostsForBlog(id),
                    BlogPosts = GetPosts(postIds),
                    Blog = blog
                };

            ViewBag.PageIndex = page;

            return View(model);
        }

        public ActionResult BlogMonth(int id, int year, int month)
        {
            var earliestDate = new DateTime(year, month, 1);
            var latestDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            var blog = _repository.Find<Blog>().Where(x => x.Id == id).Execute();

            var postIds = _repository.Find<BlogPost>()
                .Select(x => x.Id)
                .Where(x => x.Blog.Id == id && x.PublishDate >= earliestDate && x.PublishDate <= latestDate)
                .OrderByDescending(x => x.PublishDate)
                .ExecuteScalarList<int>();

            var model = new BlogModel
                {
                    Months = GetMonthsWithPostsForBlog(id),
                    BlogPosts = GetPosts(postIds),
                    Blog = blog
                };

            return View("Blog", model);
        }

        public ActionResult BlogSearch(int id, string searchString)
        {
            var blog = _repository.Find<Blog>().Where(x => x.Id == id).Execute();

            var postIds = _repository.Find<BlogPost>()
                .Select(x => x.Id)
                .Where(x => x.Blog.Id == id && (x.Content.Contains(searchString) || x.Title.Contains(searchString)))
                .OrderByDescending(x => x.PublishDate)
                .ExecuteScalarList<int>();

            var model = new BlogModel
                {
                    Months = GetMonthsWithPostsForBlog(id),
                    BlogPosts = GetPosts(postIds),
                    Blog = blog
                };

            return View("Blog", model);
        }

        private IList<BlogPost> GetPosts(IList<int> postIds)
        {
            IList<BlogPost> posts = new List<BlogPost>();

            if (postIds.Any())
            {
                posts = _repository
                    .Find<BlogPost>()
                    .OrderByDescending(x => x.PublishDate)
                    .Where(x => postIds.Contains(x.Id))
                    .Join<User, BlogPost>(x => x.BlogPosts, x => x.Author)
                    .ExecuteList();
            }
            return posts;
        }

        private IEnumerable<DateTime> GetMonthsWithPostsForBlog(int id)
        {
            var publishDates = _repository.Find<BlogPost>()
                .Select(x => x.PublishDate)
                .Where(x => x.Blog.Id == id)
                .OrderByDescending(x => x.PublishDate)
                .ExecuteScalarList<DateTime>();

            return publishDates.Select(x => new DateTime(x.Year, x.Month, 1)).Distinct();
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
            var month = 12 - (index * 2) / 28;
            var day = 28 - (index * 2) % 28;
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

            for (int i = 0; i < random.Next(10); i++)
            {
                var comment = new Comment
                    {
                        Content = "Comment " + i,
                        PublishDate = new DateTime(2011, month, day, random.Next(23), random.Next(59), random.Next(59))
                    };

                post.AddComment(comment);
            }

            post.Content = post.Title + " content";

            return post;
        }
    }
}
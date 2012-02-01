using System;
using System.Web.Mvc;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.ExampleSite.Controllers
{
    public class PostController : Controller
    {
        private readonly Repository _repository = new Repository();

        public ActionResult Index(int id)
        {
            var post = _repository.Find<BlogPost>()
                .Where(x => x.Id == id)
                .Join<User, BlogPost>(x => x.BlogPosts, x => x.Author)
                .Execute();

            var comments = _repository.Find<Comment>()
                .Where(x => x.BlogPost.Id == post.Id)
                .OrderByDescending(x => x.PublishDate)
                .ExecuteList();

            post.Comments = comments;

            return View(post);
        }

        public ActionResult Create(int id)
        {
            var post = new BlogPost
                {
                    PublishDate = DateTime.Now,
                };

            ViewBag.BlogId = id;

            return View(post);
        }

        [HttpPost]
        public ActionResult Create(BlogPost post, int blogId)
        {
            var user = _repository.Find<User>().Top(1).Execute();

            post.Blog = new Blog() { Id = blogId };
            post.Author = user;

            _repository.Insert(post);

            return RedirectToAction("Index", new { id = post.Id });
        }

        public ActionResult Edit(int id)
        {
            var post = _repository.Find<BlogPost>()
                .Where(x => x.Id == id)
                .Execute();

            return View(post);
        }

        [HttpPost]
        public ActionResult Edit(BlogPost post)
        {
            _repository.Update<BlogPost>()
                .Set(x => x.Title, post.Title)
                .Set(x => x.Content, post.Content)
                .Set(x => x.PublishDate, post.PublishDate)
                .Where(x => x.Id == post.Id)
                .Execute();

            return RedirectToAction("Index", new { id = post.Id });
        }

        public ActionResult Delete(int id)
        {
            _repository.Delete<BlogPost>().Where(x => x.Id == id).Execute();
            return RedirectToAction("Index", "Home");
        }
    }
}
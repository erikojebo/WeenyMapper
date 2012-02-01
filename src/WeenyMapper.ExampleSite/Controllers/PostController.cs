using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.ExampleSite.Controllers
{
    public class PostController : Controller
    {
        Repository _repository = new Repository();

        public ActionResult Index(int id)
        {
            var post = _repository.Find<BlogPost>()
                .Where(x => x.Id == id)
                .Join<User,BlogPost>(x => x.BlogPosts, x => x.Author)
                .Execute();

            return View(post);
        }

        //
        // GET: /Post/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Post/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /Post/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Post/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            _repository.Delete<BlogPost>().Where(x => x.Id == id).Execute();
            return RedirectToAction("Index", "Home");
        }
    }
}

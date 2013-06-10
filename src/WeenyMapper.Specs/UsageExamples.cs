using System;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class UsageExamples : AcceptanceSpecsBase
    {
        protected override void PerformSetUp()
        {
            DeleteAllExistingTestData();
        }

        [Test]
        public void Join_example_with_transaction_insert()
        {
            Repository.DefaultConnectionString = @"Data source=.\SQLEXPRESS;Initial Catalog=WeenyMapper;Trusted_Connection=true";
            Repository.DefaultConvention = new BlogConvention();

            var myBlog = new Blog("My blog");

            var steve = new User("Steve", "password");

            var post1 = new BlogPost("Title 1", "Content 1 goes here") { Blog = myBlog, Author = steve };
            var post2 = new BlogPost("Title 2", "Content 2 goes here") { Blog = myBlog, Author = steve };
            var post3 = new BlogPost("Title 3", "Content 3 goes here") { Blog = myBlog, Author = steve };
            var post4 = new BlogPost("Title 4", "Content 4 goes here") { Blog = myBlog, Author = steve };

            using (var repository = new Repository())
            {
                using (var transaction = repository.BeginTransaction())
                {
                    repository.Insert(myBlog);
                    repository.Insert(steve);
                    repository.Insert(post1, post2, post3, post4);

                    transaction.Commit();
                }

                repository.Update<BlogPost>()
                          .Set(x => x.Title, "Updated title 2")
                          .Where(x => x.Id == post2.Id)
                          .Execute();

                repository.Delete<BlogPost>()
                          .Where(x => x.Id == post3.Id || x.Title == "Title 4")
                          .Execute();

                var actualBlog = repository.Find<Blog>().Where(x => x.Id == myBlog.Id)
                                           .Join(x => x.Posts, x => x.Blog)
                                           .OrderBy<BlogPost>(x => x.Title)
                                           .Execute();

                Assert.AreEqual("My blog", actualBlog.Name);
                Assert.AreEqual(2, actualBlog.Posts.Count);
                Assert.AreEqual("Title 1", actualBlog.Posts[0].Title);
                Assert.AreEqual("Updated title 2", actualBlog.Posts[1].Title);
            }
        }

        [Test]
        public void In_query_example()
        {
            CreateBlogTestData();

            using (var repository = new Repository())
            {
                var postTitles = new[] { "Title 1", "Title 3", "Title 4" };

                var posts = repository.Find<BlogPost>()
                                      .Where(x => postTitles.Contains(x.Title))
                                      .OrderBy<BlogPost>(x => x.Title)
                                      .ExecuteList();

                Assert.AreEqual(3, posts.Count);
                Assert.AreEqual("Title 1", posts[0].Title);
                Assert.AreEqual("Title 3", posts[1].Title);
                Assert.AreEqual("Title 4", posts[2].Title);
            }
        }

        [Test]
        public void Like_queries()
        {
            CreateBlogTestData();

            using (var repository = new Repository())
            {
                var posts = repository.Find<BlogPost>()
                                      .Where(x => x.Title.StartsWith("Foo"))
                                      .ExecuteList();
                
                posts = repository.Find<BlogPost>()
                                          .Where(x => x.Title.EndsWith("bar"))
                                          .ExecuteList();
            
                posts = repository.Find<BlogPost>()
                                          .Where(x => x.Title.Contains("baz"))
                                          .ExecuteList();
            }
        }

        [Test]
        public void Numeric_comparison()
        {
            CreateBlogTestData();

            using (var repository = new Repository())
            {
var posts = repository.Find<BlogPost>()
                        .Where(x => x.PublishDate > new DateTime(2012, 1, 2) && x.PublishDate <= new DateTime(2012, 5, 6))
                        .OrderBy(x => x.PublishDate)
                        .ExecuteList();

                Assert.AreEqual(2, posts.Count);
                Assert.AreEqual("Title 2", posts[0].Title);
                Assert.AreEqual("Title 3", posts[1].Title);
            }
        }

        private static void CreateBlogTestData()
        {
            Repository.DefaultConvention = new BlogConvention();

            var myBlog = new Blog("My blog");

            var steve = new User("Steve", "password");

            var post1 = new BlogPost("Title 1", "Content 1 goes here") { Blog = myBlog, Author = steve, PublishDate = new DateTime(2011, 1, 1) };
            var post2 = new BlogPost("Title 2", "Content 2 goes here") { Blog = myBlog, Author = steve, PublishDate = new DateTime(2012, 1, 5) };
            var post3 = new BlogPost("Title 3", "Content 3 goes here") { Blog = myBlog, Author = steve, PublishDate = new DateTime(2012, 4, 1) };
            var post4 = new BlogPost("Title 4", "Content 4 goes here") { Blog = myBlog, Author = steve, PublishDate = new DateTime(2013, 1, 1) };

            using (var repository = new Repository())
            {
                using (var transaction = repository.BeginTransaction())
                {
                    repository.Insert(myBlog);
                    repository.Insert(steve);
                    repository.Insert(post1, post2, post3, post4);

                    transaction.Commit();
                }
            }
        }
    }
}
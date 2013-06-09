using NUnit.Framework;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class UsageExamples : AcceptanceSpecsBase
    {
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
    }
}
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs
{
    public class AcceptanceSpecsBase
    {
        protected Repository Repository;

        [SetUp]
        public void SetUp()
        {
            Repository = new Repository { ConnectionString = TestConnectionString };

            Repository.Convention = new DefaultConvention();
            Repository.EnableSqlConsoleLogging();

            PerformSetUp();
        }

        public virtual string TestConnectionString
        {
            get { return @"Data source=.\SQLEXPRESS;Initial Catalog=WeenyMapper;Trusted_Connection=true"; }
        }

        protected virtual void PerformSetUp() {}

        protected void DeleteAllExistingTestData()
        {
            Repository.Convention = new BlogConvention();

            Repository.Delete<Comment>().Execute();
            Repository.Delete<BlogPost>().Execute();
            Repository.Delete<Blog>().Execute();

            Repository.Convention = new UserConvention();

            Repository.Delete<User>().Execute();

            Repository.Convention = new BookConvention();

            Repository.Delete<Book>().Execute();

            Repository.Convention = new DefaultConvention();

            Repository.Delete<Movie>().Execute();
            Repository.Delete<Employee>().Execute();
            Repository.Delete<Company>().Execute();
            Repository.Delete<Event>().Execute();
        }
    }
}
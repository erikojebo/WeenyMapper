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
            Repository.DefaultConnectionString = TestConnectionString;
            Repository = new Repository();

            Repository.DefaultConvention = new DefaultConvention();
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
            Repository.DefaultConvention = new BlogConvention();

            Repository.Delete<Comment>().Execute();
            Repository.Delete<BlogPost>().Execute();
            Repository.Delete<Blog>().Execute();

            Repository.DefaultConvention = new UserConvention();

            Repository.Delete<User>().Execute();

            Repository.DefaultConvention = new BookConvention();

            Repository.Delete<Book>().Execute();

            Repository.DefaultConvention = new DefaultConvention();

            Repository.Delete<Movie>().Execute();
            Repository.Delete<Employee>().Execute();
            Repository.Delete<Company>().Execute();
            Repository.Delete<Event>().Execute();
        }
    }
}
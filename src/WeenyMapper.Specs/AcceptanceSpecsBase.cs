using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs
{
    public class AcceptanceSpecsBase
    {
        protected Repository Repository;
        public const string TestConnectionString = @"Data source=.\SQLEXPRESS;Initial Catalog=WeenyMapper;Trusted_Connection=true";

        protected void DeleteAllExistingTestData()
        {
            Repository = new Repository { ConnectionString = TestConnectionString };

            Repository.Convention = new UserConvention();

            Repository.Delete<User>().Execute();

            Repository.Convention = new BookConvention();

            Repository.Delete<Book>().Execute();

            Repository.Convention = new DefaultConvention();

            Repository.Delete<Movie>().Execute();
        }
    }
}
using System;
using NUnit.Framework;
using WeenyMapper.Specs.Entities;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class RepositoryAcceptanceSpecs
    {
        /*
         
         Requirements for running these tests:
         SQL Server Express instance with a database called WeenyMapper
          
         The database should be created from the script file at SqlScripts/CreateTestDatabase.sql
         
         */

        public const string TestConnectionString = @"Data source=.\SQLEXPRESS;Initial Catalog=WeenyMapper;Trusted_Connection=true";

        [SetUp]
        public void SetUp() {}

        [Test]
        public void An_object_can_be_inserted_into_the_database_and_read_back_via_a_dynamic_query_on_the_given_id()
        {
            var repository = new Repository { ConnectionString = TestConnectionString };
            var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "a username",
                    Password = "a password"
                };

            repository.Insert.User(user);
            var actualUser = repository.Find.UserById(user.Id).As<User>();

            Assert.AreEqual(user.Id, actualUser.Id);
            Assert.AreEqual("a username", actualUser.Username);
            Assert.AreEqual("a password", actualUser.Password);
        }
    }
}
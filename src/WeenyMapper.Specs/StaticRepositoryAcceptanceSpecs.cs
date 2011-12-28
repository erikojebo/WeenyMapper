using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.Entities;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class StaticRepositoryAcceptanceSpecs :AcceptanceSpecsBase
    {
        /*
         
                 Requirements for running these tests:
                 SQL Server Express instance with a database called WeenyMapper
          
                 The database should be created from the script file at SqlScripts/CreateTestDatabase.sql
         
                 */

        private StaticRepository _repository;

        [SetUp]
        public void SetUp()
        {
            Repository.Convention = new DefaultConvention();

            DeleteAllExistingTestData();

            _repository = new StaticRepository { ConnectionString = TestConnectionString };
        }

        [Test]
        public void An_object_can_be_inserted_into_the_database_and_read_back_via_a_dynamic_query_on_the_given_id()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "a username",
                Password = "a password"
            };

            _repository.Insert(user);
            var actualUser = _repository.Find<User>().By(x => x.Id, user.Id).Execute();

            Assert.AreEqual(user.Id, actualUser.Id);
            Assert.AreEqual("a username", actualUser.Username);
            Assert.AreEqual("a password", actualUser.Password);
        }

    }

}
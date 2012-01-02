using System;
using System.Threading;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class AsyncRepositoryAcceptanceSpecs : AcceptanceSpecsBase
    {
        /*
         
         Requirements for running these tests:
         SQL Server Express instance with a database called WeenyMapper
          
         The database should be created from the script file at SqlScripts/CreateTestDatabase.sql
         
         */

        [SetUp]
        public void SetUp()
        {
            DeleteAllExistingTestData();

            Repository.Convention = new DefaultConvention();
            Repository.EnableSqlConsoleLogging();
        }

        [Timeout(5000)]
        [Test]
        public void Insert_for_single_entity_can_be_run_asynchronously()
        {
            var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "a username",
                    Password = "a password"
                };

            AssertCallbackIsInvoked(callback => Repository.InsertAsync(user, callback));

            var actualUser = Repository.Find<User>().Where(x => x.Id, user.Id).Execute();
            Assert.AreEqual(user, actualUser);
        }

        [Timeout(5000)]
        [Test]
        public void Insert_for_multiple_entities_can_be_run_asynchronously()
        {
            var user1 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username1",
                    Password = "a password"
                };

            var user2 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username2",
                    Password = "a password"
                };

            AssertCallbackIsInvoked(callback => Repository.InsertManyAsync(new[] { user1, user2 }, callback));

            var actualUsers = Repository.Find<User>().ExecuteList();

            Assert.AreEqual(2, actualUsers.Count);
            CollectionAssert.Contains(actualUsers, user1);
            CollectionAssert.Contains(actualUsers, user2);
        }

        [Timeout(5000)]
        [Test]
        public void Update_of_single_entity_can_be_run_asynchronously()
        {
            var user1 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username1",
                    Password = "a password"
                };

            Repository.Insert(user1);

            user1.Username = "Updated username";

            AssertCallbackIsInvoked(callback => Repository.UpdateAsync(user1, callback));

            var actualUser = Repository.Find<User>().Where(x => x.Id, user1.Id).Execute();

            Assert.AreEqual(user1, actualUser);
        }

        [Timeout(5000)]
        [Test]
        public void Update_for_multiple_entities_can_be_run_asynchronously()
        {
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);
            var actualRowCount = 0;

            var user1 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username1",
                    Password = "a password"
                };

            var user2 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username2",
                    Password = "another password"
                };

            var user3 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username3",
                    Password = "a password"
                };

            Repository.InsertMany(user1, user2, user3);

            AssertParameterizedCallbackIsInvoked(2,
                callback =>
                Repository.Update<User>()
                    .Where(x => x.Password, "a password")
                    .Set(x => x.Password, "updated password")
                    .ExecuteAsync(callback));

            var actualUsers = Repository.Find<User>()
                .Where(x => x.Password, "updated password")
                .ExecuteList();

            Assert.AreEqual(2, actualUsers.Count);
            Assert.AreEqual("updated password", actualUsers[0].Password);
            Assert.AreEqual("updated password", actualUsers[1].Password);
        }

        [Timeout(5000)]
        [Test]
        public void Delete_of_single_entity_can_be_run_asynchronously()
        {
            var user1 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username1",
                    Password = "a password"
                };

            Repository.Insert(user1);

            user1.Username = "Updated username";

            AssertCallbackIsInvoked(callback => Repository.DeleteAsync(user1, callback));

            var actualUserCount = Repository.Count<User>().Execute();

            Assert.AreEqual(0, actualUserCount);
        }

        [Timeout(5000)]
        [Test]
        public void Delete_for_multiple_entities_can_be_run_asynchronously()
        {
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);
            var actualRowCount = 0;

            var user1 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username1",
                    Password = "a password"
                };

            var user2 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username2",
                    Password = "another password"
                };

            var user3 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username3",
                    Password = "a password"
                };

            Repository.InsertMany(user1, user2, user3);

            AssertParameterizedCallbackIsInvoked(2,
                callback =>
                Repository.Delete<User>()
                    .Where(x => x.Password, "a password")
                    .ExecuteAsync(callback));

            var userCount = Repository.Count<User>().Execute();

            Assert.AreEqual(1, userCount);
        }

        private void AssertCallbackIsInvoked(Action<Action> operation)
        {
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

            operation(() => wasCallbackCalledSemaphore.Release());

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();
        }

        private void AssertParameterizedCallbackIsInvoked<T>(T expectedParameter, Action<Action<T>> operation)
        {
            var actualParameter = default(T);
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

            operation(parameter =>
                {
                    actualParameter = parameter;
                    wasCallbackCalledSemaphore.Release();
                });

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

            Assert.AreEqual(expectedParameter, actualParameter);
        }
    }
}
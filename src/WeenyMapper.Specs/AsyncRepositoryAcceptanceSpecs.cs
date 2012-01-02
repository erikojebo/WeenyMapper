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
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "a username",
                Password = "a password"
            };

            Repository.InsertAsync(user, () => wasCallbackCalledSemaphore.Release(1));

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

            var actualUser = Repository.Find<User>().Where(x => x.Id, user.Id).Execute();
            Assert.AreEqual(user, actualUser);
        }

        [Timeout(5000)]
        [Test]
        public void Insert_for_multiple_entities_can_be_run_asynchronously()
        {
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

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

            Repository.InsertManyAsync(new[] { user1, user2 }, () => wasCallbackCalledSemaphore.Release());

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

            var actualUsers = Repository.Find<User>().ExecuteList();

            Assert.AreEqual(2, actualUsers.Count);
            CollectionAssert.Contains(actualUsers, user1);
            CollectionAssert.Contains(actualUsers, user2);
        }

        [Timeout(5000)]
        [Test]
        public void Update_of_single_entity_can_be_run_asynchronously()
        {
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Username = "username1",
                Password = "a password"
            };

            Repository.Insert(user1);

            user1.Username = "Updated username";

            Repository.UpdateAsync(user1, () => wasCallbackCalledSemaphore.Release());

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

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

            Repository.Update<User>().Where(x => x.Password, "a password")
                .Set(x => x.Password, "updated password")
                .ExecuteAsync(rowCount =>
                {
                    actualRowCount = rowCount;
                    wasCallbackCalledSemaphore.Release();
                });

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

            var actualUsers = Repository.Find<User>()
                .Where(x => x.Password, "updated password")
                .ExecuteList();

            Assert.AreEqual(2, actualRowCount);
            Assert.AreEqual(2, actualUsers.Count);
            Assert.AreEqual("updated password", actualUsers[0].Password);
            Assert.AreEqual("updated password", actualUsers[1].Password);
        }

        [Timeout(5000)]
        [Test]
        public void Delete_of_single_entity_can_be_run_asynchronously()
        {
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Username = "username1",
                Password = "a password"
            };

            Repository.Insert(user1);

            user1.Username = "Updated username";

            Repository.DeleteAsync(user1, () => wasCallbackCalledSemaphore.Release());

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

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

            Repository.Delete<User>().Where(x => x.Password, "a password")
                .ExecuteAsync(rowCount =>
                {
                    actualRowCount = rowCount;
                    wasCallbackCalledSemaphore.Release();
                });

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

            var userCount = Repository.Count<User>().Execute();

            Assert.AreEqual(2, actualRowCount);
            Assert.AreEqual(1, userCount);
        }
    }
}
using System;
using System.Collections.Generic;
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

        protected override void PerformSetUp()
        {
            DeleteAllExistingTestData();
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

            var actualUser = Repository.Find<User>().Where(x => x.Id == user.Id).Execute();
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

            AssertCallbackIsInvoked(callback => Repository.InsertCollectionAsync(new[] { user1, user2 }, callback));

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

            var actualUser = Repository.Find<User>().Where(x => x.Id == user1.Id).Execute();

            Assert.AreEqual(user1, actualUser);
        }

        [Timeout(5000)]
        [Test]
        public void Update_for_multiple_entities_can_be_run_asynchronously()
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
                    Password = "another password"
                };

            var user3 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username3",
                    Password = "a password"
                };

            Repository.Insert(user1, user2, user3);

            AssertParameterizedCallbackIsInvoked(2,
                callback =>
                Repository.Update<User>()
                    .Where(x => x.Password == "a password")
                    .Set(x => x.Password, "updated password")
                    .ExecuteAsync(callback));

            var actualUsers = Repository.Find<User>()
                .Where(x => x.Password == "updated password")
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

            Repository.Insert(user1, user2, user3);

            AssertParameterizedCallbackIsInvoked(2,
                callback =>
                Repository.Delete<User>()
                    .Where(x => x.Password == "a password")
                    .ExecuteAsync(callback));

            var userCount = Repository.Count<User>().Execute();

            Assert.AreEqual(1, userCount);
        }

        [Timeout(5000)]
        [Test]
        public void Find_of_single_entity_can_be_run_asynchronously()
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
                    Password = "another password"
                };

            Repository.Insert(user1, user2);

            AssertParameterizedCallbackIsInvoked(user1,
                callback => Repository.Find<User>()
                                .Where(x => x.Id == user1.Id)
                                .ExecuteAsync(callback));
        }

        [Timeout(5000)]
        [Test]
        public void Find_of_multiple_entities_can_be_run_asynchronously()
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
                    Password = "another password"
                };

            var user3 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username3",
                    Password = "a password"
                };

            Repository.Insert(user1, user2, user3);

            AssertListCallbackIsInvoked(new[] { user1, user3 },
                callback => Repository.Find<User>()
                                .Where(x => x.Password == "a password")
                                .ExecuteListAsync(callback));
        }

        [Timeout(5000)]
        [Test]
        public void Find_scalar_can_be_run_asynchronously()
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
                    Password = "another password"
                };

            Repository.Insert(user1, user2);

            AssertParameterizedCallbackIsInvoked("username2",
                callback => Repository.Find<User>()
                                .Where(x => x.Id == user2.Id)
                                .Select(x => x.Username)
                                .ExecuteScalarAsync(callback));
        }

        [Timeout(5000)]
        [Test]
        public void Find_scalar_for_multiple_entities_can_be_run_asynchronously()
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
                    Password = "another password"
                };

            var user3 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username3",
                    Password = "a password"
                };

            Repository.Insert(user1, user2, user3);

            AssertListCallbackIsInvoked(new[] { "username1", "username3" },
                callback => Repository.Find<User>()
                                .Where(x => x.Password == "a password")
                                .Select(x => x.Username)
                                .ExecuteScalarListAsync(callback));
        }

        [Timeout(5000)]
        [Test]
        public void Entities_with_identity_generated_ids_gets_assigned_id_written_to_id_property_after_insert_callback_is_called()
        {
            var movie1 = new Movie
                {
                    Title = "Movie 1",
                    ReleaseDate = new DateTime(2012, 1, 2)
                };

            var movie2 = new Movie
                {
                    Title = "Movie 2",
                    ReleaseDate = new DateTime(2012, 1, 2)
                };

            var movie3 = new Movie
                {
                    Title = "Movie 3",
                    ReleaseDate = new DateTime(2012, 1, 2)
                };

            AssertCallbackIsInvoked(x => Repository.InsertAsync(movie1, x));
            AssertCallbackIsInvoked(x => Repository.InsertCollectionAsync(new[] { movie2, movie3 }, x));

            var allMovies = Repository.Find<Movie>().OrderBy(x => x.Title).ExecuteList();

            Assert.AreEqual(3, allMovies.Count);
            Assert.AreEqual(allMovies[0].Id, movie1.Id);
            Assert.AreEqual(allMovies[1].Id, movie2.Id);
            Assert.AreEqual(allMovies[2].Id, movie3.Id);
        }

        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_inserting_single_entity()
        {
            AssertErrorCallbackIsInvoked((c, e) => Repository.InsertAsync(new EntityWithoutTable(), c, e));
        }

        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_inserting_multiple_entities()
        {
            var entities = new[] { new EntityWithoutTable(), new EntityWithoutTable() };
            AssertErrorCallbackIsInvoked((c, e) => Repository.InsertCollectionAsync(entities, c, e));
        }
        
        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_updating_single_entity()
        {
            AssertErrorCallbackIsInvoked((c, e) => Repository.UpdateAsync(new EntityWithoutTable(), c, e));
        }

        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_updating_multiple_entities()
        {
            AssertErrorCallbackForParameterizedResultCallbackIsInvoked<int>(
                (c, e) => Repository.Update<EntityWithoutTable>()
                    .Where(x => x.Id == 0)
                    .Set(x => x.Id, 1).ExecuteAsync(c, e));
        }
        
        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_deleting_single_entity()
        {
            AssertErrorCallbackIsInvoked((c, e) => Repository.DeleteAsync(new EntityWithoutTable(), c, e));
        }

        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_deleting_multiple_entities()
        {
            AssertErrorCallbackForParameterizedResultCallbackIsInvoked<int>(
                (c, e) => Repository.Delete<EntityWithoutTable>()
                    .Where(x => x.Id == 0)
                    .ExecuteAsync(c, e));
        }

        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_finding_single_entity()
        {
            AssertErrorCallbackForParameterizedResultCallbackIsInvoked<EntityWithoutTable>(
                (c, e) => Repository
                    .Find<EntityWithoutTable>()
                    .Where(x => x.Id == 1)
                    .ExecuteAsync(c, e));
        }

        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_finding_multiple_entities()
        {
            AssertErrorCallbackForParameterizedResultCallbackIsInvoked<IList<EntityWithoutTable>>(
                (c, e) => Repository
                    .Find<EntityWithoutTable>()
                    .ExecuteListAsync(c, e));
        }
        
        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_finding_single_scalar()
        {
            AssertErrorCallbackForParameterizedResultCallbackIsInvoked<EntityWithoutTable>(
                (c, e) => Repository
                    .Find<EntityWithoutTable>()
                    .Where(x => x.Id == 1)
                    .Select(x => x.Id)
                    .ExecuteScalarAsync(c, e));
        }

        [Timeout(5000)]
        [Test]
        public void Error_callback_is_called_on_exception_while_finding_list_of_scalars()
        {
            AssertErrorCallbackForParameterizedResultCallbackIsInvoked<IList<EntityWithoutTable>>(
                (c, e) => Repository
                    .Find<EntityWithoutTable>()
                    .Select(x => x.Id)
                    .ExecuteScalarListAsync(c, e));
        }

        private void AssertCallbackIsInvoked(Action<Action> operation)
        {
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

            operation(() => wasCallbackCalledSemaphore.Release());

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();
        }

        private void AssertErrorCallbackIsInvoked(Action<Action, Action<Exception>> operation)
        {
            var wasErrorCallbackCalledSemaphore = new Semaphore(0, 1);

            operation(() => { }, _ => wasErrorCallbackCalledSemaphore.Release());

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasErrorCallbackCalledSemaphore.WaitOne();
        }
        
        private void AssertErrorCallbackForParameterizedResultCallbackIsInvoked<T>(Action<Action<T>, Action<Exception>> operation)
        {
            var wasErrorCallbackCalledSemaphore = new Semaphore(0, 1);

            operation(_ => { }, _ => wasErrorCallbackCalledSemaphore.Release());

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasErrorCallbackCalledSemaphore.WaitOne();
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

        private void AssertListCallbackIsInvoked<T>(T[] expectedEntities, Action<Action<IList<T>>> operation)
        {
            IList<T> actualParameter = new List<T>();
            var wasCallbackCalledSemaphore = new Semaphore(0, 1);

            operation(parameter =>
                {
                    actualParameter = parameter;
                    wasCallbackCalledSemaphore.Release();
                });

            // Wait until callback is called. The test will fail if the timeout is reached)
            wasCallbackCalledSemaphore.WaitOne();

            Assert.AreEqual(expectedEntities.Length, actualParameter.Count);

            foreach (var expectedEntity in expectedEntities)
            {
                CollectionAssert.Contains(actualParameter, expectedEntity);
            }
        }
    }
}
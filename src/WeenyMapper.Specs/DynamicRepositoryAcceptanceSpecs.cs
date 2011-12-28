using System;
using System.Data.SqlClient;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.Entities;
using WeenyMapper.Specs.TestClasses.Conventions;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class DynamicRepositoryAcceptanceSpecs : AcceptanceSpecsBase
    {
        /*
         
         Requirements for running these tests:
         SQL Server Express instance with a database called WeenyMapper
          
         The database should be created from the script file at SqlScripts/CreateTestDatabase.sql
         
         */

        private Repository _repository;

        [SetUp]
        public void SetUp()
        {
            Repository.Convention = new DefaultConvention();

            DeleteAllExistingTestData();

            _repository = new Repository { ConnectionString = TestConnectionString };
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

            _repository.Insert.User(user);
            var actualUser = _repository.Find.UserById(user.Id).Execute<User>();

            Assert.AreEqual(user.Id, actualUser.Id);
            Assert.AreEqual("a username", actualUser.Username);
            Assert.AreEqual("a password", actualUser.Password);
        }

        [Test]
        public void Multiple_properties_can_be_used_when_querying_for_objects()
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
            
            var user3 = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "username3",
                    Password = "a password"
                };

            _repository.Insert.User(user1);
            _repository.Insert.User(user2);
            _repository.Insert.User(user3);

            var actualUser = _repository.Find.UserByPasswordAndUsername(user2.Password, user2.Username).Execute<User>();

            Assert.AreEqual(user2.Id, actualUser.Id);
            Assert.AreEqual("username2", actualUser.Username);
            Assert.AreEqual("a password", actualUser.Password);
        }

        [Test]
        public void Updating_an_object_updates_the_database_entry_with_the_corresponding_id()
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

            _repository.Insert.User(user1);
            _repository.Insert.User(user2);

            var updatedUser2 = new User
                {
                    Id = user2.Id,
                    Username = "updated username",
                    Password = "updated password"
                };

            _repository.Update.User(updatedUser2);

            var actualUser = _repository.Find.UserById(updatedUser2.Id).Execute<User>();

            Assert.AreEqual(updatedUser2.Id, actualUser.Id);
            Assert.AreEqual("updated username", actualUser.Username);
            Assert.AreEqual("updated password", actualUser.Password);
        }

        [Test]
        public void Subset_of_the_columns_of_a_table_can_be_read_by_specifying_a_target_type_which_contains_properties_matching_the_subset()
        {
            var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "a username",
                    Password = "a password"
                };

            _repository.Insert.User(user);
            var actualUser = _repository.Find.UserById(user.Id).Execute<PartialUser>();

            Assert.AreEqual(user.Id, actualUser.Id);
            Assert.AreEqual("a username", actualUser.Username);
        }

        [Test]
        public void Object_with_table_and_columns_using_non_default_conventions_can_be_written_updated_and_read()
        {
            Repository.Convention = new BookConvention();

            var book = new Book
                {
                    Isbn = "123-456",
                    Title = "Book title",
                    AuthorName = "The Author Name",
                    PageCount = 123
                };

            _repository.Insert.Book(book);

            Book readBook = _repository.Find.BookByIsbn(book.Isbn).Execute<Book>();
            
            readBook.Title = "Updated book title";
            readBook.AuthorName = "Updated author name";

            _repository.Update.Book(readBook);

            Book readUpdatedBook = _repository.Find
                .BookByTitleAndAuthorName("Updated book title", "Updated author name")
                .Execute<Book>();

            Assert.AreEqual("123-456", readUpdatedBook.Isbn);
            Assert.AreEqual("Updated book title", readUpdatedBook.Title);
            Assert.AreEqual("Updated author name", readUpdatedBook.AuthorName);
            Assert.AreEqual(123, readUpdatedBook.PageCount);
        }
    }
}
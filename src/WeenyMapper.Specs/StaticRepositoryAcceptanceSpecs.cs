using System;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.Entities;
using WeenyMapper.Specs.TestClasses.Conventions;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class StaticRepositoryAcceptanceSpecs : AcceptanceSpecsBase
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

        [Test]
        public void Multiple_properties_can_be_used_when_querying_for_objects()
        {
            StaticRepository.Convention = new BookConvention();

            var book1 = new Book
            {
                Isbn = "1",
                AuthorName = "Author Name",
                Title = "Title 1",
                PageCount = 123,
            };

            var book2 = new Book
            {
                Isbn = "2",
                AuthorName = "Author Name",
                Title = "Title 2",
                PageCount = 123
            };

            var book3 = new Book
            {
                Isbn = "3",
                AuthorName = "Author Name",
                Title = "Title 3",
                PageCount = 123
            };

            _repository.Insert(book1);
            _repository.Insert(book2);
            _repository.Insert(book3);

            Book actualBook = _repository.Find<Book>()
                .By(x => x.AuthorName, "Author Name")
                .By(x => x.Title, "Title 2")
                .By(x => x.PageCount, 123)
                .Execute();

            Assert.AreEqual(book2.Isbn, actualBook.Isbn);
            Assert.AreEqual("Author Name", actualBook.AuthorName);
            Assert.AreEqual("Title 2", actualBook.Title);
            Assert.AreEqual(123, actualBook.PageCount);
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

            _repository.Insert(user1);
            _repository.Insert(user2);

            var updatedUser2 = new User
                {
                    Id = user2.Id,
                    Username = "updated username",
                    Password = "updated password"
                };

            _repository.Update(updatedUser2);

            var actualUser = _repository.Find<User>().By(x => x.Id, updatedUser2.Id).Execute();

            Assert.AreEqual(updatedUser2.Id, actualUser.Id);
            Assert.AreEqual("updated username", actualUser.Username);
            Assert.AreEqual("updated password", actualUser.Password);
        }

        [Test]
        public void Subset_of_the_columns_of_a_table_can_be_read_by_specifying_a_target_type_which_contains_properties_matching_the_subset()
        {
            StaticRepository.Convention = new UserConvention();

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
        }

        [Test]
        public void Object_with_table_and_columns_using_non_default_conventions_can_be_written_updated_and_read()
        {
            StaticRepository.Convention = new BookConvention();

            var book = new Book
                {
                    Isbn = "123-456",
                    Title = "Book title",
                    AuthorName = "The Author Name",
                    PageCount = 123
                };

            _repository.Insert(book);

            var readBook = _repository.Find<Book>().By(x => x.Isbn, book.Isbn).Execute();

            readBook.Title = "Updated book title";
            readBook.AuthorName = "Updated author name";

            _repository.Update(readBook);

            var readUpdatedBook = _repository.Find<Book>()
                .By(x => x.Title, "Updated book title")
                .By(x => x.AuthorName, "Updated author name")
                .Execute();

            Assert.AreEqual("123-456", readUpdatedBook.Isbn);
            Assert.AreEqual("Updated book title", readUpdatedBook.Title);
            Assert.AreEqual("Updated author name", readUpdatedBook.AuthorName);
            Assert.AreEqual(123, readUpdatedBook.PageCount);
        }
    }
}
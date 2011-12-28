using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class RepositoryAcceptanceSpecs : AcceptanceSpecsBase
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

            _repository.Insert(user);
            var actualUser = _repository.Find<User>().By(x => x.Id, user.Id).Execute();

            Assert.AreEqual(user, actualUser);
        }

        [Test]
        public void Multiple_properties_can_be_used_when_querying_for_objects()
        {
            Repository.Convention = new BookConvention();

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

            Assert.AreEqual(book2, actualBook);
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

            Assert.AreEqual(updatedUser2, actualUser);
        }

        [Test]
        public void Subset_of_the_columns_of_a_table_can_be_read_by_specifying_a_target_type_which_contains_properties_matching_the_subset()
        {
            Repository.Convention = new UserConvention();

            var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "a username",
                    Password = "a password"
                };

            _repository.Insert(user);
            var actualUser = _repository.Find<PartialUser>().By(x => x.Id, user.Id).Execute();

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

            _repository.Insert(book);

            var readBook = _repository.Find<Book>().By(x => x.Isbn, book.Isbn).Execute();

            readBook.Title = "Updated book title";
            readBook.AuthorName = "Updated author name";

            _repository.Update(readBook);

            var readUpdatedBook = _repository.Find<Book>()
                .By(x => x.Title, "Updated book title")
                .By(x => x.AuthorName, "Updated author name")
                .Execute();

            Assert.AreEqual(readBook, readUpdatedBook);
        }

        [Test]
        public void Multiple_matching_objects_can_be_read_with_a_single_query()
        {
            Repository.Convention = new BookConvention();

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
                AuthorName = "Author Name 2",
                Title = "Title 2",
                PageCount = 123
            };

            var book3 = new Book
            {
                Isbn = "3",
                AuthorName = "Author Name 2",
                Title = "Title 3",
                PageCount = 123
            };

            var book4 = new Book
            {
                Isbn = "4",
                AuthorName = "Author Name 2",
                Title = "Title 4",
                PageCount = 321
            };

            _repository.Insert(book1);
            _repository.Insert(book2);
            _repository.Insert(book3);
            _repository.Insert(book4);

            var actualBooks = _repository.Find<Book>()
                .By(x => x.AuthorName, "Author Name 2")
                .By(x => x.PageCount, 123)
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);

            CollectionAssert.Contains(actualBooks, book2);
            CollectionAssert.Contains(actualBooks, book3);
        }

        [Test]
        public void Update_commands_can_be_issued_to_update_multiple_entities_at_once()
        {
            Repository.Convention = new BookConvention();

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
                AuthorName = "Author Name 2",
                Title = "Title 2",
                PageCount = 123
            };

            var book3 = new Book
            {
                Isbn = "3",
                AuthorName = "Author Name 2",
                Title = "Title 3",
                PageCount = 123
            };

            var book4 = new Book
            {
                Isbn = "4",
                AuthorName = "Author Name 2",
                Title = "Title 4",
                PageCount = 321
            };

            _repository.Insert(book1);
            _repository.Insert(book2);
            _repository.Insert(book3);
            _repository.Insert(book4);

            _repository.Update<Book>()
                .Where(x => x.AuthorName, "Author Name 2")
                .Where(x => x.PageCount, 123)
                .Set(x => x.PageCount, 456)
                .Set(x => x.Title, "new title")
                .Execute();

            var actualBooks = _repository.Find<Book>()
                .By(x => x.PageCount, 456)
                .By(x => x.Title, "new title")
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);

            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "2"));
            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "3"));
        }
    }
}

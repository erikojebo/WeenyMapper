using System;
using System.Collections.Generic;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Conventions;
using System.Linq;
using WeenyMapper.Specs.TestClasses.Entities;

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

        [SetUp]
        public void SetUp()
        {
            DeleteAllExistingTestData();

            Repository.Convention = new DefaultConvention();
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

            Repository.Insert(user);
            var actualUser = Repository.DynamicFind<User>().ById(user.Id).Execute();

            Assert.AreEqual(user.Id, actualUser.Id);
            Assert.AreEqual("a username", actualUser.Username);
            Assert.AreEqual("a password", actualUser.Password);
        }

        [Test]
        public void Multiple_properties_can_be_used_when_querying_for_objects_by_chaining_constraint_calls()
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

            Repository.Insert(book1);
            Repository.Insert(book2);
            Repository.Insert(book3);

            Book actualBook = Repository.DynamicFind<Book>()
                .ByAuthorName("Author Name")
                .ByTitle("Title 2")
                .ByPageCount(123)
                .Execute();

            Assert.AreEqual(book2, actualBook);
        }

        [Test]
        public void Multiple_properties_can_be_used_when_querying_for_objects_by_specifying_constraints_on_the_format_ByUsernameAndPassword()
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

            Repository.Insert(book1);
            Repository.Insert(book2);
            Repository.Insert(book3);

            Book actualBook = Repository.DynamicFind<Book>()
                .ByAuthorNameAndTitleAndPageCount("Author Name", "Title 2", 123)
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

            Repository.Insert(user1);
            Repository.Insert(user2);

            var updatedUser2 = new User
                {
                    Id = user2.Id,
                    Username = "updated username",
                    Password = "updated password"
                };

            Repository.Update(updatedUser2);

            var actualUser = Repository.DynamicFind<User>().ById(updatedUser2.Id).Execute();

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

            Repository.Insert(user);
            var actualUser = Repository.DynamicFind<PartialUser>().ById(user.Id).Execute();

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

            Repository.Insert(book);

            Book readBook = Repository.DynamicFind<Book>().ByIsbn(book.Isbn).Execute();

            readBook.Title = "Updated book title";
            readBook.AuthorName = "Updated author name";

            Repository.Update(readBook);

            Book readUpdatedBook = Repository.DynamicFind<Book>()
                .ByTitle("Updated book title")
                .ByAuthorName("Updated author name")
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

            Repository.Insert(book1);
            Repository.Insert(book2);
            Repository.Insert(book3);
            Repository.Insert(book4);

            IList<Book> actualBooks = Repository.DynamicFind<Book>()
                .ByAuthorName("Author Name 2")
                .ByPageCount(123)
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

            Repository.Insert(book1);
            Repository.Insert(book2);
            Repository.Insert(book3);
            Repository.Insert(book4);

            Repository.DynamicUpdate<Book>()
                .WhereAuthorName("Author Name 2")
                .WherePageCount(123)
                .SetPageCount(456)
                .SetTitle("new title")
                .Execute();

            IList<Book> actualBooks = Repository.DynamicFind<Book>()
                .ByPageCount(456)
                .ByTitle("new title")
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);

            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "2"));
            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "3"));
        }

        [Test]
        public void Delete_commands_can_be_issued_to_update_multiple_entities_at_once()
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

            Repository.Insert(book1);
            Repository.Insert(book2);
            Repository.Insert(book3);
            Repository.Insert(book4);

            Repository.DynamicDelete<Book>()
                .WhereAuthorName("Author Name 2")
                .WherePageCount(123)
                .Execute();

            var allBooks = Repository.Find<Book>().ExecuteList();

            // Only the books NOT matching the query shold remain
            Assert.AreEqual(2, allBooks.Count);
            CollectionAssert.Contains(allBooks, book1);
            CollectionAssert.Contains(allBooks, book4);
        }

    }
}
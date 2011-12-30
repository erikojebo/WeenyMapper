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
    public class DynamicRepositoryAcceptanceSpecs : AcceptanceSpecsBase
    {
        /* Requirements for running these tests:
           SQL Server Express instance with a database called WeenyMapper
          
           The database should be created from the script file at SqlScripts/CreateTestDatabase.sql */

        [SetUp]
        public void SetUp()
        {
            DeleteAllExistingTestData();

            Repository.Convention = new DefaultConvention();
            Repository.EnableSqlConsoleLogging();
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
            var actualUser = Repository.DynamicFind<User>().WhereId(user.Id).Execute();

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
                .WhereAuthorName("Author Name")
                .WhereTitle("Title 2")
                .WherePageCount(123)
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
                .WhereAuthorNameAndTitleAndPageCount("Author Name", "Title 2", 123)
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

            var affectedRowCount = Repository.Update(updatedUser2);

            var actualUser = Repository.DynamicFind<User>().WhereId(updatedUser2.Id).Execute();

            Assert.AreEqual(1, affectedRowCount);
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
            var actualUser = Repository.DynamicFind<PartialUser>().WhereId(user.Id).Execute();

            Assert.AreEqual(user.Id, actualUser.Id);
            Assert.AreEqual("a username", actualUser.Username);
        }

        [Test]
        public void Object_with_table_and_columns_using_non_default_conventions_can_be_written_updated_and_read()
        {
            Repository.Convention = new BookConvention();

            var book1 = new Book
                {
                    Isbn = "1",
                    Title = "Book title 1",
                    AuthorName = "Author Name",
                    PageCount = 123
                };

            var book2 = new Book
                {
                    Isbn = "2",
                    Title = "Book title 2",
                    AuthorName = "Author Name 2",
                    PageCount = 123
                };

            var book3 = new Book
                {
                    Isbn = "3",
                    Title = "Book title 3",
                    AuthorName = "Author Name 2",
                    PageCount = 123
                };

            var book4 = new Book
                {
                    Isbn = "4",
                    Title = "Book title 4",
                    AuthorName = "Author Name",
                    PageCount = 123
                };

            Repository.Insert(book1);
            Repository.Insert(book2);
            Repository.Insert(book3);
            Repository.Insert(book4);

            Book readBook = Repository.DynamicFind<Book>().WhereIsbn(book1.Isbn).Execute();

            readBook.Title = "Updated book title";
            readBook.AuthorName = "Updated author name";

            Repository.Update(readBook);

            Book readUpdatedBook = Repository.DynamicFind<Book>()
                .WhereTitle("Updated book title")
                .WhereAuthorName("Updated author name")
                .Execute();

            var updatedRowCount = Repository.DynamicUpdate<Book>().WhereAuthorName("Author Name 2")
                .SetPageCount(456)
                .Execute();

            IList<Book> updatedBooks = Repository.DynamicFind<Book>()
                .WherePageCount(456)
                .ExecuteList();

            var deletedRowCount = Repository.DynamicDelete<Book>()
                .WherePageCount(456)
                .Execute();

            Repository.Delete(book1);

            var booksAfterDelete = Repository.DynamicFind<Book>().ExecuteList();

            Assert.AreEqual(2, updatedRowCount);
            Assert.AreEqual(2, deletedRowCount);
            Assert.AreEqual(2, updatedBooks.Count);
            Assert.AreEqual(1, booksAfterDelete.Count);
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
                .WhereAuthorName("Author Name 2")
                .WherePageCount(123)
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
                .WherePageCount(456)
                .WhereTitle("new title")
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

        [Test]
        public void The_number_of_items_satisfying_a_series_of_constraints_can_be_read_with_a_count_query()
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

            Repository.InsertMany(book1, book2, book3, book4);

            int count = Repository.DynamicCount<Book>()
                .WhereAuthorName("Author Name 2")
                .WherePageCount(123)
                .Execute();

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Partial_object_can_be_read_by_explicitly_specifying_which_columns_to_fetch()
        {
            Repository.Convention = new BookConvention();

            var book1 = new Book
                {
                    Isbn = "1",
                    AuthorName = "Author Name",
                    Title = "Title 1",
                    PageCount = 123,
                };

            Repository.Insert(book1);

            var partialBook = Repository.DynamicFind<Book>()
                .WhereIsbn("1")
                .SelectIsbn()
                .SelectTitle()
                .Execute();

            Assert.AreEqual("1", partialBook.Isbn);
            Assert.AreEqual("Title 1", partialBook.Title);
            Assert.AreEqual(0, partialBook.PageCount);
            Assert.IsNull(partialBook.AuthorName);
        }

        [Test]
        public void Multiple_partial_objects_can_be_read_by_explicitly_specifying_which_columns_to_fetch()
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

            Repository.InsertMany(book1, book2, book3);

            IList<Book> partialBooks = Repository.DynamicFind<Book>()
                .WhereAuthorName("Author Name 2")
                .SelectIsbn()
                .SelectTitle()
                .ExecuteList();

            partialBooks = partialBooks.OrderBy(x => x.Isbn).ToList();

            Assert.AreEqual(2, partialBooks.Count);

            Assert.AreEqual("2", partialBooks[0].Isbn);
            Assert.AreEqual("Title 2", partialBooks[0].Title);
            Assert.AreEqual(0, partialBooks[0].PageCount);
            Assert.IsNull(partialBooks[0].AuthorName);

            Assert.AreEqual("3", partialBooks[1].Isbn);
            Assert.AreEqual("Title 3", partialBooks[1].Title);
            Assert.AreEqual(0, partialBooks[1].PageCount);
            Assert.IsNull(partialBooks[1].AuthorName);
        }
    }
}
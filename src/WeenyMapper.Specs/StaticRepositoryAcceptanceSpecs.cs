using System;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

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
            var actualUser = Repository.Find<User>().Where(x => x.Id, user.Id).Execute();

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

            Repository.Insert(book1);
            Repository.Insert(book2);
            Repository.Insert(book3);

            Book actualBook = Repository.Find<Book>()
                .Where(x => x.AuthorName, "Author Name")
                .Where(x => x.Title, "Title 2")
                .Where(x => x.PageCount, 123)
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

            var actualUser = Repository.Find<User>().Where(x => x.Id, updatedUser2.Id).Execute();

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
            var actualUser = Repository.Find<PartialUser>().Where(x => x.Id, user.Id).Execute();

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

            var readBook = Repository.Find<Book>().Where(x => x.Isbn, book1.Isbn).Execute();

            readBook.Title = "Updated book title";
            readBook.AuthorName = "Updated author name";

            Repository.Update(readBook);

            var readUpdatedBook = Repository.Find<Book>()
                .Where(x => x.Title, "Updated book title")
                .Where(x => x.AuthorName, "Updated author name")
                .Execute();

            var updatedRowCount = Repository.Update<Book>().Where(x => x.AuthorName, "Author Name 2")
                .Set(x => x.PageCount, 456)
                .Execute();

            var updatedBooks = Repository.Find<Book>()
                .Where(x => x.PageCount, 456)
                .ExecuteList();

            var deletedRowCount = Repository.Delete<Book>()
                .Where(x => x.PageCount, 456)
                .Execute();

            Repository.Delete(book1);

            var booksAfterDelete = Repository.Find<Book>().ExecuteList();

            Assert.AreEqual(2, updatedRowCount);
            Assert.AreEqual(2, updatedBooks.Count);
            Assert.AreEqual(2, deletedRowCount);
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

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.AuthorName, "Author Name 2")
                .Where(x => x.PageCount, 123)
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

            Repository.Update<Book>()
                .Where(x => x.AuthorName, "Author Name 2")
                .Where(x => x.PageCount, 123)
                .Set(x => x.PageCount, 456)
                .Set(x => x.Title, "new title")
                .Execute();

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.PageCount, 456)
                .Where(x => x.Title, "new title")
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);

            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "2"));
            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "3"));
        }

        [Test]
        public void Deleting_an_entity_deletes_the_corresponding_row_and_no_other_row()
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

            Repository.Delete(user2);

            var actualUsers = Repository.Find<User>().ExecuteList();

            Assert.AreEqual(1, actualUsers.Count);
            Assert.AreEqual(user1, actualUsers[0]);
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

            Repository.Delete<Book>()
                .Where(x => x.AuthorName, "Author Name 2")
                .Where(x => x.PageCount, 123)
                .Execute();

            var allBooks = Repository.Find<Book>().ExecuteList();

            // Only the books NOT matching the query shold remain
            Assert.AreEqual(2, allBooks.Count);
            CollectionAssert.Contains(allBooks, book1);
            CollectionAssert.Contains(allBooks, book4);
        }

        [Test]
        public void Multiple_entities_can_be_inserted_with_one_operation()
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

            Repository.InsertMany(new[] { book1, book2 });

            var actualBooks = Repository.Find<Book>().ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.Contains(actualBooks, book1);
            CollectionAssert.Contains(actualBooks, book2);
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

            int count = Repository.Count<Book>()
                .Where(x => x.AuthorName, "Author Name 2")
                .Where(x => x.PageCount, 123)
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

            var partialBook = Repository.Find<Book>()
                .Where(x => x.Isbn, "1")
                .Select(x => x.Isbn)
                .Select(x => x.Title)
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

            var partialBooks = Repository.Find<Book>()
                .Where(x => x.AuthorName, "Author Name 2")
                .Select(x => x.Isbn)
                .Select(x => x.Title)
                .ExecuteList()
                .OrderBy(x => x.Isbn).ToList();

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

        [Test]
        public void Find_query_can_be_evaluated_to_a_single_scalar_value()
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

            Repository.InsertMany(user1, user2);

            var actualUsername = Repository.Find<User>()
                .Where(x => x.Id, user2.Id)
                .Select(x => x.Username)
                .ExecuteScalar<string>();

            Assert.AreEqual("username2", actualUsername);
        }

        [Test]
        public void Scalar_values_can_be_returned_for_find_query_matching_multiple_entities()
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

            Repository.InsertMany(user1, user2, user3);

            var usernames = Repository.Find<User>()
                .Where(x => x.Password, "a password")
                .Select(x => x.Username)
                .ExecuteScalarList<string>();

            Assert.AreEqual(2, usernames.Count);
            CollectionAssert.Contains(usernames, "username1");
            CollectionAssert.Contains(usernames, "username3");
        }

        [Test]
        public void Expressions_can_be_used_to_run_queries()
        {
            Repository.Convention = new BookConvention();

            var book1 = new Book
            {
                Isbn = "1",
                AuthorName = "Author Name",
                Title = "Title 1",
                PageCount = 100,
            };

            var book2 = new Book
            {
                Isbn = "2",
                AuthorName = "Another Author Name",
                Title = "Title 2",
                PageCount = 200
            };

            var book3 = new Book
            {
                Isbn = "3",
                AuthorName = "Another Author Name",
                Title = "Title 3",
                PageCount = 150
            };
            
            var book4 = new Book
            {
                Isbn = "4",
                AuthorName = "Another Author Name",
                Title = "Title 4",
                PageCount = 75
            };

            var book5 = new Book
            {
                Isbn = "5",
                AuthorName = "Author Name",
                Title = "Title 5",
                PageCount = 75
            };
            
            var book6 = new Book
            {
                Isbn = "6",
                AuthorName = "Author Name",
                Title = "Title 6",
                PageCount = 50
            };

            Repository.InsertMany(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.Isbn == "6" || (x.PageCount == 75 && x.AuthorName == "Another Author Name"))
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.Contains(actualBooks, book4);
            CollectionAssert.Contains(actualBooks, book6);
        }

        [Test]
        public void Linq_contains_call_within_composite_query_expressions_can_be_used_to_run_sql_in_queries()
        {
            Repository.Convention = new BookConvention();

            var book1 = new Book
            {
                Isbn = "1",
                AuthorName = "Author Name",
                Title = "Title 1",
                PageCount = 100,
            };

            var book2 = new Book
            {
                Isbn = "2",
                AuthorName = "Another Author Name",
                Title = "Title 2",
                PageCount = 200
            };

            var book3 = new Book
            {
                Isbn = "3",
                AuthorName = "Another Author Name",
                Title = "Title 3",
                PageCount = 150
            };
            
            var book4 = new Book
            {
                Isbn = "4",
                AuthorName = "Another Author Name",
                Title = "Title 4",
                PageCount = 75
            };

            var book5 = new Book
            {
                Isbn = "5",
                AuthorName = "Author Name",
                Title = "Title 5",
                PageCount = 75
            };
            
            var book6 = new Book
            {
                Isbn = "6",
                AuthorName = "Author Name",
                Title = "Title 6",
                PageCount = 50
            };

            Repository.InsertMany(book1, book2, book3, book4, book5, book6);

            var titles = new[] { "Title 4", "Title 5", "Title 6" };

            var actualBooks = Repository.Find<Book>()
                .Where(x => titles.Contains(x.Title) && x.AuthorName == "Author Name")
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.Contains(actualBooks, book5);
            CollectionAssert.Contains(actualBooks, book6);
        }
    }
}
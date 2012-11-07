using System;
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

        protected override void PerformSetUp()
        {
            Repository.DatabaseProvider = new SqlServerDatabaseProvider();
            DeleteAllExistingTestData();
        }

        [Test]
        public virtual void An_object_can_be_inserted_into_the_database_and_read_back_via_a_query_on_the_given_id()
        {
            var user = new User
                           {
                               Id = Guid.NewGuid(),
                               Username = "a username",
                               Password = "a password"
                           };

            Repository.Insert(user);
            var actualUser = Repository.Find<User>().Where(x => x.Id == user.Id).Execute();

            Assert.AreEqual(user, actualUser);
        }

        [Test]
        public virtual void Query_for_single_entity_without_any_match_returns_null()
        {
            var user = new User
                           {
                               Id = Guid.NewGuid(),
                               Username = "a username",
                               Password = "a password"
                           };

            Repository.Insert(user);

            var nonExistingId = Guid.NewGuid();
            var actualUser = Repository.Find<User>().Where(x => x.Id == nonExistingId).Execute();

            Assert.IsNull(actualUser);
        }

        [Test]
        public virtual void Multiple_properties_can_be_used_when_querying_for_objects()
        {
            Repository.DefaultConvention = new BookConvention();

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
                .Where(x => x.AuthorName == "Author Name" && x.Title == "Title 2" && x.PageCount == 123)
                .Execute();

            Assert.AreEqual(book2, actualBook);
        }

        [Test]
        public virtual void Multiple_where_clauses_can_be_combined_using_And_when_querying_for_objects()
        {
            Repository.DefaultConvention = new BookConvention();

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
                .Where(x => x.AuthorName == "Author Name")
                .Where(x => x.Title == "Title 2")
                .Where(x => x.PageCount == 123)
                .Execute();

            Assert.AreEqual(book2, actualBook);
        }

        [Test]
        public virtual void Multiple_where_clauses_can_be_combined_using_Or_when_querying_for_objects()
        {
            Repository.DefaultConvention = new BookConvention();

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

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.Isbn == "1")
                .OrWhere(x => x.Title == "Title 2")
                .OrderBy(x => x.Isbn)
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            Assert.AreEqual(book1, actualBooks[0]);
            Assert.AreEqual(book2, actualBooks[1]);
        }

        [Test]
        public virtual void Multiple_where_clauses_can_be_combined_using_both_And_and_Or_when_querying_for_objects()
        {
            Repository.DefaultConvention = new BookConvention();

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
                                PageCount = 222
                            };

            var book4 = new Book
                            {
                                Isbn = "4",
                                AuthorName = "Author Name",
                                Title = "Title 4",
                                PageCount = 222
                            };

            var book5 = new Book
                            {
                                Isbn = "5",
                                AuthorName = "Author Name",
                                Title = "Title 5",
                                PageCount = 123
                            };

            var book6 = new Book
                            {
                                Isbn = "6",
                                AuthorName = "Another author Name",
                                Title = "Title 6",
                                PageCount = 222
                            };

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.Isbn == "1")
                .OrWhere(x => x.PageCount == 222)
                .AndWhere(x => x.AuthorName == "Author Name")
                .OrderBy(x => x.Isbn)
                .ExecuteList();

            Assert.AreEqual(3, actualBooks.Count);
            Assert.AreEqual(book1, actualBooks[0]);
            Assert.AreEqual(book3, actualBooks[1]);
            Assert.AreEqual(book4, actualBooks[2]);
        }

        [Test]
        public virtual void Updating_an_object_updates_the_database_entry_with_the_corresponding_id()
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

            var actualUser = Repository.Find<User>().Where(x => x.Id == updatedUser2.Id).Execute();

            Assert.AreEqual(updatedUser2, actualUser);
        }

        [Test]
        public virtual void Subset_of_the_columns_of_a_table_can_be_read_by_specifying_a_target_type_which_contains_properties_matching_the_subset()
        {
            Repository.DefaultConvention = new UserConvention();

            var user = new User
                           {
                               Id = Guid.NewGuid(),
                               Username = "a username",
                               Password = "a password"
                           };

            Repository.Insert(user);
            var actualUser = Repository.Find<PartialUser>().Where(x => x.Id == user.Id).Execute();

            Assert.AreEqual(user.Id, actualUser.Id);
            Assert.AreEqual("a username", actualUser.Username);
        }

        [Test]
        public virtual void Object_with_table_and_columns_using_non_default_conventions_can_be_written_updated_and_read()
        {
            Repository.DefaultConvention = new BookConvention();

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

            var readBook = Repository.Find<Book>().Where(x => x.Isbn == book1.Isbn).Execute();

            readBook.Title = "Updated book title";
            readBook.AuthorName = "Updated author name";

            Repository.Update(readBook);

            var readUpdatedBook = Repository.Find<Book>()
                .Where(x => x.Title == "Updated book title")
                .Where(x => x.AuthorName == "Updated author name")
                .Execute();

            var updatedRowCount = Repository.Update<Book>().Where(x => x.AuthorName == "Author Name 2")
                .Set(x => x.PageCount, 456)
                .Execute();

            var updatedBooks = Repository.Find<Book>()
                .Where(x => x.PageCount == 456)
                .ExecuteList();

            var deletedRowCount = Repository.Delete<Book>()
                .Where(x => x.PageCount == 456)
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
        public virtual void Multiple_matching_objects_can_be_read_with_a_single_query()
        {
            Repository.DefaultConvention = new BookConvention();

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
                .Where(x => x.AuthorName == "Author Name 2" && x.PageCount == 123)
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);

            CollectionAssert.Contains(actualBooks, book2);
            CollectionAssert.Contains(actualBooks, book3);
        }

        [Test]
        public virtual void Update_commands_can_be_issued_to_update_multiple_entities_at_once()
        {
            Repository.DefaultConvention = new BookConvention();

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
                .Where(x => x.AuthorName == "Author Name 2" && x.PageCount == 123)
                .Set(x => x.PageCount, 456)
                .Set(x => x.Title, "new title")
                .Execute();

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.PageCount == 456 && x.Title == "new title")
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);

            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "2"));
            Assert.AreEqual(1, actualBooks.Count(x => x.Isbn == "3"));
        }

        [Test]
        public virtual void Deleting_an_entity_deletes_the_corresponding_row_and_no_other_row()
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
        public virtual void Delete_commands_can_be_issued_to_update_multiple_entities_at_once()
        {
            Repository.DefaultConvention = new BookConvention();

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
                .Where(x => x.AuthorName == "Author Name 2" && x.PageCount == 123)
                .Execute();

            var allBooks = Repository.Find<Book>().ExecuteList();

            // Only the books NOT matching the query shold remain
            Assert.AreEqual(2, allBooks.Count);
            CollectionAssert.Contains(allBooks, book1);
            CollectionAssert.Contains(allBooks, book4);
        }

        [Test]
        public virtual void Multiple_entities_can_be_inserted_with_one_operation()
        {
            Repository.DefaultConvention = new BookConvention();

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

            Repository.Insert(new[] { book1, book2 });

            var actualBooks = Repository.Find<Book>().ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.Contains(actualBooks, book1);
            CollectionAssert.Contains(actualBooks, book2);
        }

        [Test]
        public virtual void The_number_of_items_satisfying_a_series_of_constraints_can_be_read_with_a_count_query()
        {
            Repository.DefaultConvention = new BookConvention();

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

            Repository.Insert(book1, book2, book3, book4);

            int count = Repository.Count<Book>()
                .Where(x => x.AuthorName == "Author Name 2" && x.PageCount == 123)
                .Execute();

            Assert.AreEqual(2, count);
        }

        [Test]
        public virtual void Partial_object_can_be_read_by_explicitly_specifying_which_columns_to_fetch()
        {
            Repository.DefaultConvention = new BookConvention();

            var book1 = new Book
                            {
                                Isbn = "1",
                                AuthorName = "Author Name",
                                Title = "Title 1",
                                PageCount = 123,
                            };

            Repository.Insert(book1);

            var partialBook = Repository.Find<Book>()
                .Where(x => x.Isbn == "1")
                .Select(x => x.Isbn)
                .Select(x => x.Title)
                .Execute();

            Assert.AreEqual("1", partialBook.Isbn);
            Assert.AreEqual("Title 1", partialBook.Title);
            Assert.AreEqual(0, partialBook.PageCount);
            Assert.IsNull(partialBook.AuthorName);
        }

        [Test]
        public virtual void Multiple_partial_objects_can_be_read_by_explicitly_specifying_which_columns_to_fetch()
        {
            Repository.DefaultConvention = new BookConvention();

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

            Repository.Insert(book1, book2, book3);

            var partialBooks = Repository.Find<Book>()
                .Where(x => x.AuthorName == "Author Name 2")
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
        public virtual void Find_query_can_be_evaluated_to_a_single_scalar_value()
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

            var actualUsername = Repository.Find<User>()
                .Where(x => x.Id == user2.Id)
                .Select(x => x.Username)
                .ExecuteScalar<string>();

            Assert.AreEqual("username2", actualUsername);
        }

        [Test]
        public virtual void Scalar_values_can_be_returned_for_find_query_matching_multiple_entities()
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

            var usernames = Repository.Find<User>()
                .Where(x => x.Password == "a password")
                .Select(x => x.Username)
                .ExecuteScalarList<string>();

            Assert.AreEqual(2, usernames.Count);
            CollectionAssert.Contains(usernames, "username1");
            CollectionAssert.Contains(usernames, "username3");
        }

        [Test]
        public virtual void Expressions_can_be_used_to_run_queries()
        {
            Repository.DefaultConvention = new BookConvention();

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

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.Isbn == "6" || (x.PageCount == 75 && x.AuthorName == "Another Author Name"))
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.Contains(actualBooks, book4);
            CollectionAssert.Contains(actualBooks, book6);
        }

        [Test]
        public virtual void Linq_contains_call_within_composite_query_expressions_can_be_used_to_run_sql_in_queries()
        {
            Repository.DefaultConvention = new BookConvention();

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

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var titles = new[] { "Title 4", "Title 5", "Title 6" };

            var actualBooks = Repository.Find<Book>()
                .Where(x => titles.Contains(x.Title) && x.AuthorName == "Author Name")
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.Contains(actualBooks, book5);
            CollectionAssert.Contains(actualBooks, book6);
        }

        [Test]
        public virtual void Only_properties_matching_ShouldMap_predicate_in_current_convention_should_be_read_and_written_to_the_database()
        {
            Repository.DefaultConvention = new UserWithExtraPropertiesConvention();

            var user1 = new UserWithExtraProperties
                            {
                                Id = Guid.NewGuid(),
                                Username = "Extra user 1",
                                Password = "Password",
                                PublicExtraProperty = "Extra property value"
                            };

            var user2 = new UserWithExtraProperties
                            {
                                Id = Guid.NewGuid(),
                                Username = "Extra user 2",
                                Password = "Password",
                                PublicExtraProperty = "Extra property value"
                            };

            var user3 = new UserWithExtraProperties
                            {
                                Id = Guid.NewGuid(),
                                Username = "Extra user 3",
                                Password = "Password",
                                PublicExtraProperty = "Extra property value"
                            };

            Repository.Insert(user1);
            Repository.Insert(user2, user3);

            user1.Password = "Updated password";

            Repository.Update<User>(user1);
            Repository.Update<User>().Where(x => x.Password == "Password").Set(x => x.Password, "Another updated password").Execute();

            var batchUpdatedUsers = Repository.Find<User>().Where(x => x.Password == "Another updated password").ExecuteList()
                .OrderBy(x => x.Username).ToList();

            Repository.Delete(user3);
            Repository.Delete<UserWithExtraProperties>().Where(x => x.Username == "Extra user 2").Execute();

            var finalUsers = Repository.Find<User>().ExecuteList();

            Assert.AreEqual(1, finalUsers.Count);
            Assert.AreEqual(user1, finalUsers.First());

            Assert.AreEqual(2, batchUpdatedUsers.Count);
            Assert.AreEqual("Extra user 2", batchUpdatedUsers.First().Username);
            Assert.AreEqual("Another updated password", batchUpdatedUsers.First().Password);
            Assert.AreEqual("Extra user 3", batchUpdatedUsers.Last().Username);
            Assert.AreEqual("Another updated password", batchUpdatedUsers.Last().Password);
        }

        [Test]
        public virtual void Default_convention_only_maps_non_static_public_read_write_properties()
        {
            Repository.DefaultConvention = new DefaultConvention();

            var movie = new Movie
                            {
                                Title = "Movie title",
                                ReleaseDate = new DateTime(2012, 01, 18)
                            };

            movie.SetRating(4);

            Repository.Insert(movie);

            var actualMovie = Repository.Find<Movie>().Where(x => x.Title == "Movie title").Execute();

            Assert.AreEqual(movie.Title, actualMovie.Title);
            Assert.AreEqual(movie.ReleaseDate, actualMovie.ReleaseDate);
            Assert.AreEqual(0, actualMovie.Rating); // should not be mapped
        }

        [Test]
        public virtual void Result_from_find_query_can_be_ordered_by_multiple_columns()
        {
            Repository.DefaultConvention = new BookConvention();

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
                                PageCount = 75
                            };

            var book4 = new Book
                            {
                                Isbn = "4",
                                AuthorName = "Another Author Name",
                                Title = "Title 4",
                                PageCount = 150
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
                                PageCount = 75
                            };

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .OrderBy(x => x.AuthorName)
                .OrderByDescending(x => x.PageCount, x => x.Title)
                .ExecuteList();

            Assert.AreEqual(6, actualBooks.Count);
            CollectionAssert.AreEqual(new[] { book2, book4, book3, book1, book6, book5 }, actualBooks);
        }

        [Test]
        public virtual void Result_count_can_be_limited_by_using_Top()
        {
            Repository.DefaultConvention = new BookConvention();

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
                                PageCount = 75
                            };

            var book4 = new Book
                            {
                                Isbn = "4",
                                AuthorName = "Another Author Name",
                                Title = "Title 4",
                                PageCount = 150
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
                                PageCount = 75
                            };

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .OrderByDescending(x => x.Isbn)
                .Top(3)
                .ExecuteList();

            Assert.AreEqual(3, actualBooks.Count);
            CollectionAssert.AreEqual(new[] { book6, book5, book4 }, actualBooks);
        }

        [Test]
        public virtual void Part_of_the_result_can_be_returned_from_find_query_by_specifying_page_number_and_size()
        {
            Repository.DefaultConvention = new BookConvention();

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
                                PageCount = 75
                            };

            var book4 = new Book
                            {
                                Isbn = "4",
                                AuthorName = "Another Author Name",
                                Title = "Title 4",
                                PageCount = 150
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
                                PageCount = 75
                            };

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .OrderByDescending(x => x.Isbn)
                .Page(1, 2)
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.AreEqual(new[] { book4, book3 }, actualBooks);
        }

        [Test]
        public virtual void Paging_query_without_explicit_ordering_orders_by_primary_key()
        {
            Repository.DefaultConvention = new BookConvention();

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
                                PageCount = 75
                            };

            var book4 = new Book
                            {
                                Isbn = "4",
                                AuthorName = "Another Author Name",
                                Title = "Title 4",
                                PageCount = 150
                            };

            Repository.Insert(book1, book2, book3, book4);

            var actualBooks = Repository.Find<Book>()
                .Page(1, 2)
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.AreEqual(new[] { book3, book4 }, actualBooks);
        }

        [Test]
        public virtual void String_contains_call_can_be_used_to_execute_like_query()
        {
            Repository.DefaultConvention = new BookConvention();

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
                                AuthorName = "Steven Smith",
                                Title = "Title 2",
                                PageCount = 200
                            };

            var book3 = new Book
                            {
                                Isbn = "3",
                                AuthorName = "Smith Stevenson",
                                Title = "Title 3",
                                PageCount = 75
                            };

            var book4 = new Book
                            {
                                Isbn = "4",
                                AuthorName = "Another Author Name",
                                Title = "Title 4",
                                PageCount = 150
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
                                AuthorName = "Fred-Steve Smith",
                                Title = "Title 6",
                                PageCount = 75
                            };

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.AuthorName.Contains("Steve"))
                .OrderBy(x => x.Isbn)
                .ExecuteList();

            Assert.AreEqual(3, actualBooks.Count);
            CollectionAssert.AreEqual(new[] { book2, book3, book6 }, actualBooks);
        }

        [Test]
        public virtual void String_StartsWith_and_EndsWith_call_can_be_used_to_execute_like_queries()
        {
            Repository.DefaultConvention = new BookConvention();

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
                                AuthorName = "Steven Smith",
                                Title = "Title 2",
                                PageCount = 200
                            };

            var book3 = new Book
                            {
                                Isbn = "3",
                                AuthorName = "Smith Stevenson",
                                Title = "Title 3",
                                PageCount = 75
                            };

            var book4 = new Book
                            {
                                Isbn = "4",
                                AuthorName = "Another Author Name",
                                Title = "Title 4",
                                PageCount = 150
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
                                AuthorName = "Fred-Steve Smith",
                                Title = "Title 6",
                                PageCount = 75
                            };

            Repository.Insert(book1, book2, book3, book4, book5, book6);

            var actualBooks = Repository.Find<Book>()
                .Where(x => x.AuthorName.StartsWith("Smith") || (x.AuthorName.EndsWith("Smith") && x.Title.EndsWith("6")))
                .OrderBy(x => x.Isbn)
                .ExecuteList();

            Assert.AreEqual(2, actualBooks.Count);
            CollectionAssert.AreEqual(new[] { book3, book6 }, actualBooks);
        }

        [Test]
        public virtual void Entities_with_identity_generated_ids_gets_assigned_id_written_to_id_property_after_insert()
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

            Repository.Insert(movie1);
            Repository.Insert(movie2, movie3);

            var allMovies = Repository.Find<Movie>().OrderBy(x => x.Title).ExecuteList();

            Assert.AreEqual(3, allMovies.Count);
            Assert.AreEqual(allMovies[0].Id, movie1.Id);
            Assert.AreEqual(allMovies[1].Id, movie2.Id);
            Assert.AreEqual(allMovies[2].Id, movie3.Id);
        }

        [Test]
        public virtual void Many_to_one_relationship_can_be_written_and_read_back_again_in_single_query_using_join()
        {
            Repository.DefaultConvention = new BlogConvention();

            var blog1 = new Blog
                            {
                                Name = "Blog 1",
                            };

            var blog2 = new Blog
                            {
                                Name = "Blog 2",
                            };

            var post1 = new BlogPost
                            {
                                Title = "Blog post 1",
                                Content = "Post 1 content",
                                PublishDate = new DateTime(2011, 1, 1),
                            };

            var post2 = new BlogPost
                            {
                                Title = "Blog post 2",
                                Content = "Post 2 content",
                                PublishDate = new DateTime(2011, 1, 2)
                            };

            var post3 = new BlogPost
                            {
                                Title = "Blog post 3",
                                Content = "Post 3 content",
                                PublishDate = new DateTime(2011, 1, 3)
                            };

            blog1.AddPost(post1);
            blog1.AddPost(post2);
            blog2.AddPost(post3);

            Repository.Insert(blog1, blog2);
            Repository.Insert(post1, post2, post3);

            var actualBlog1 = Repository.Find<Blog>().Where(x => x.Name == "Blog 1").Join<Blog, BlogPost>(x => x.Posts, x => x.Blog).Execute();

            Assert.AreEqual(blog1, actualBlog1);
        }

        [Test]
        public virtual void Multi_level_relationship_can_be_written_and_read_back_again_in_single_query_using_join()
        {
            Repository.DefaultConvention = new BlogConvention();

            var blog1 = new Blog
                            {
                                Name = "Blog 1",
                            };

            var blog2 = new Blog
                            {
                                Name = "Blog 2",
                            };

            var blog3 = new Blog
                            {
                                Name = "Blog 3",
                            };

            var blog4 = new Blog
                            {
                                Name = "Blog 4",
                            };

            var post1 = new BlogPost
                            {
                                Title = "Blog post 1",
                                Content = "Post 1 content",
                                PublishDate = new DateTime(2011, 1, 1),
                            };

            var post2 = new BlogPost
                            {
                                Title = "Blog post 2",
                                Content = "Post 2 content",
                                PublishDate = new DateTime(2011, 1, 2)
                            };

            var post3 = new BlogPost
                            {
                                Title = "Blog post 3",
                                Content = "Post 3 content",
                                PublishDate = new DateTime(2011, 1, 3)
                            };

            var post4 = new BlogPost
                            {
                                Title = "Blog post 4",
                                Content = "Post 4 content",
                                PublishDate = new DateTime(2011, 1, 4)
                            };

            var comment1 = new Comment
                               {
                                   Content = "Comment 1",
                                   PublishDate = new DateTime(2011, 1, 5)
                               };

            var comment2 = new Comment
                               {
                                   Content = "Comment 2",
                                   PublishDate = new DateTime(2011, 1, 6)
                               };

            var comment3 = new Comment
                               {
                                   Content = "Comment 3",
                                   PublishDate = new DateTime(2011, 1, 7)
                               };

            blog1.AddPost(post1);
            blog1.AddPost(post2);
            blog2.AddPost(post3);
            blog4.AddPost(post4);

            post1.AddComment(comment1);
            post2.AddComment(comment2);
            post2.AddComment(comment3);

            Repository.Insert(blog1, blog2, blog3, blog4);
            Repository.Insert(post1, post2, post3, post4);
            Repository.Insert(comment1, comment2, comment3);

            var actualBlogs = Repository.Find<Blog>().Where(x => x.Name == "Blog 1" || x.Name.EndsWith("3"))
                .OrderBy(x => x.Name)
                .Join<Blog, BlogPost>(x => x.Posts, x => x.Blog)
                .Join<BlogPost, Comment>(x => x.Comments, x => x.BlogPost)
                .ExecuteList();

            Assert.AreEqual(2, actualBlogs.Count);

            Assert.AreEqual(blog1, actualBlogs[0]);
            Assert.AreEqual(blog3, actualBlogs[1]);
        }

        [Test]
        public virtual void Multi_level_relationship_directed_from_child_to_parent_can_be_written_and_read_back_in_single_query()
        {
            Repository.DefaultConvention = new BlogConvention();

            var blog1 = new Blog
                            {
                                Name = "Blog 1",
                            };

            var blog2 = new Blog
                            {
                                Name = "Blog 2",
                            };

            var post1 = new BlogPost
                            {
                                Title = "Blog post 1",
                                Content = "Post 1 content",
                                PublishDate = new DateTime(2011, 1, 1),
                            };

            var post2 = new BlogPost
                            {
                                Title = "Blog post 2",
                                Content = "Post 2 content",
                                PublishDate = new DateTime(2011, 1, 2)
                            };

            var post3 = new BlogPost
                            {
                                Title = "Blog post 3",
                                Content = "Post 3 content",
                                PublishDate = new DateTime(2011, 1, 3)
                            };

            var comment1 = new Comment
                               {
                                   Content = "Comment 1",
                                   PublishDate = new DateTime(2011, 1, 5)
                               };

            var comment2 = new Comment
                               {
                                   Content = "Comment 2",
                                   PublishDate = new DateTime(2011, 1, 6)
                               };

            var comment3 = new Comment
                               {
                                   Content = "Comment 3",
                                   PublishDate = new DateTime(2011, 1, 7)
                               };

            var comment4 = new Comment
                               {
                                   Content = "Comment 4",
                                   PublishDate = new DateTime(2011, 1, 8)
                               };

            var comment5 = new Comment
                               {
                                   Content = "Comment 5",
                                   PublishDate = new DateTime(2011, 1, 9)
                               };

            blog1.AddPost(post1);
            blog1.AddPost(post2);
            blog2.AddPost(post3);

            post1.AddComment(comment1);
            post1.AddComment(comment2);
            post1.AddComment(comment3);
            post2.AddComment(comment4);
            post2.AddComment(comment5);

            Repository.Insert(blog1, blog2);
            Repository.Insert(post1, post2, post3);
            Repository.Insert(comment1, comment2, comment3, comment4, comment5);

            var actualComments = Repository.Find<Comment>().Where(x => x.PublishDate >= new DateTime(2011, 1, 6))
                .OrderBy(x => x.PublishDate)
                .Join<BlogPost, Comment>(x => x.Comments, x => x.BlogPost)
                .Join<Blog, BlogPost>(x => x.Posts, x => x.Blog)
                .ExecuteList();

            // Since comment1 has a PublishDate before 2011-01-06 it will not be returned by the query.
            // Because of this the instance of post1 returned will not have comment1 in its Comments collection.
            // So, to make the asserts correct. That comment is removed from the original post1.
            post1.Comments.Remove(comment1);

            Assert.AreEqual(4, actualComments.Count);

            Assert.AreEqual(comment2, actualComments[0]);
            Assert.AreEqual(comment3, actualComments[1]);
            Assert.AreEqual(comment4, actualComments[2]);
            Assert.AreEqual(comment5, actualComments[3]);

            Assert.AreEqual(post1, actualComments[0].BlogPost);
            Assert.AreEqual(post2, actualComments[2].BlogPost);

            Assert.AreEqual(blog1, actualComments[0].BlogPost.Blog);

            Assert.AreSame(actualComments[0].BlogPost, actualComments[1].BlogPost);
            Assert.AreSame(actualComments[2].BlogPost, actualComments[3].BlogPost);

            Assert.AreSame(actualComments[0].BlogPost.Blog, actualComments[1].BlogPost.Blog);
            Assert.AreSame(actualComments[0].BlogPost.Blog, actualComments[2].BlogPost.Blog);
            Assert.AreSame(actualComments[0].BlogPost.Blog, actualComments[3].BlogPost.Blog);
        }

        [Test]
        public void Entities_with_relation_mapped_as_both_reference_and_foreign_key_value_can_be_written_and_read_back_again_using_single_table_query()
        {
            var company1 = new Company
                               {
                                   Name = "Company 1"
                               };

            var employee1 = new Employee
                                {
                                    FirstName = "Steve",
                                    LastName = "Smith",
                                    BirthDate = new DateTime(1972, 1, 2),
                                    Company = company1
                                };

            var employee2 = new Employee
                                {
                                    FirstName = "John",
                                    LastName = "Johnsson",
                                    BirthDate = new DateTime(1954, 11, 12),
                                    Company = company1
                                };

            Repository.Insert(company1);
            Repository.Insert(employee1, employee2);

            var actualEmployees = Repository.Find<Employee>()
                .OrderBy(x => x.BirthDate)
                .ExecuteList();

            Assert.AreEqual(2, actualEmployees.Count);

            Assert.IsNull(actualEmployees[0].Company);
            Assert.IsNull(actualEmployees[1].Company);
            Assert.AreEqual(company1.Id, actualEmployees[0].CompanyId);
            Assert.AreEqual(company1.Id, actualEmployees[1].CompanyId);
        }

        [Test]
        public void Entities_with_relation_mapped_as_both_reference_and_foreign_key_value_can_be_written_and_read_back_again_using_join()
        {
            var company1 = new Company
                               {
                                   Name = "Company 1"
                               };
            var company2 = new Company
                               {
                                   Name = "Company 2"
                               };

            var employee1 = new Employee
                                {
                                    FirstName = "Steve",
                                    LastName = "Smith",
                                    BirthDate = new DateTime(1972, 1, 2),
                                };

            var employee2 = new Employee
                                {
                                    FirstName = "John",
                                    LastName = "Johnsson",
                                    BirthDate = new DateTime(1954, 11, 12),
                                };

            company1.AddEmployee(employee1);
            company1.AddEmployee(employee2);

            Repository.Insert(company1, company2);
            Repository.Insert(employee1, employee2);

            employee2.BirthDate = new DateTime(1965, 1, 1);

            Repository.Update(employee2);
            Repository.Update<Employee>()
                .Set(x => x.CompanyId, company2.Id)
                .Where(x => x.Id == employee2.Id)
                .Execute();

            var allCompanies = Repository.Find<Company>()
                .OrderBy(x => x.Name)
                .Join<Company, Employee>(x => x.Employees, x => x.Company)
                .ExecuteList();

            var actualEmployee2 = Repository.Find<Employee>()
                .Where(x => x.Id == employee2.Id)
                .Join<Company, Employee>(x => x.Employees, x => x.Company)
                .Execute();

            // Set up original entities to match the expected result
            company1.RemoveEmployee(employee2);
            company2.AddEmployee(employee2);

            employee1.RefreshReferencedIds();
            employee2.RefreshReferencedIds();

            Assert.AreEqual(company1, allCompanies[0]);
            Assert.AreEqual(company2, allCompanies[1]);
            Assert.AreEqual(employee2, actualEmployee2);
        }

        [Test]
        public void Update_can_be_performed_by_setting_a_reference_property_to_another_entity()
        {
            var company1 = new Company
                               {
                                   Name = "Company 1"
                               };
            var company2 = new Company
                               {
                                   Name = "Company 2"
                               };

            var employee1 = new Employee
                                {
                                    FirstName = "Steve",
                                    LastName = "Smith",
                                    BirthDate = new DateTime(1972, 1, 2),
                                };

            var employee2 = new Employee
                                {
                                    FirstName = "John",
                                    LastName = "Johnsson",
                                    BirthDate = new DateTime(1954, 11, 12),
                                };

            company1.AddEmployee(employee1);
            company1.AddEmployee(employee2);

            Repository.Insert(company1, company2);
            Repository.Insert(employee1, employee2);

            Repository.Update<Employee>()
                .Set(x => x.Company, company2)
                .Where(x => x.Id == employee2.Id)
                .Execute();

            var allCompanies = Repository.Find<Company>()
                .OrderBy(x => x.Name)
                .Join<Company, Employee>(x => x.Employees, x => x.Company)
                .ExecuteList();

            // Set up original entities to match the expected result
            company1.RemoveEmployee(employee2);
            company2.AddEmployee(employee2);

            employee1.RefreshReferencedIds();
            employee2.RefreshReferencedIds();

            Assert.AreEqual(company1, allCompanies[0]);
            Assert.AreEqual(company2, allCompanies[1]);
        }

        [Test]
        public void Different_relations_on_same_entity_can_be_loaded_in_separate_queries_using_caching_repository()
        {
            Repository.IsEntityCachingEnabled = true;

            Repository.DefaultConvention = new BlogConvention();

            var blog1 = new Blog
                            {
                                Name = "Blog 1",
                            };

            var blog2 = new Blog
                            {
                                Name = "Blog 2",
                            };

            var post1 = new BlogPost
                            {
                                Title = "Blog post 1",
                                Content = "Post 1 content",
                                PublishDate = new DateTime(2011, 1, 1),
                            };

            var post2 = new BlogPost
                            {
                                Title = "Blog post 2",
                                Content = "Post 2 content",
                                PublishDate = new DateTime(2011, 2, 2),
                            };

            var comment1 = new Comment
                               {
                                   Content = "Comment 1",
                                   PublishDate = new DateTime(2011, 1, 5)
                               };

            var comment2 = new Comment
                               {
                                   Content = "Comment 2",
                                   PublishDate = new DateTime(2011, 1, 6)
                               };

            var comment3 = new Comment
                               {
                                   Content = "Comment 2",
                                   PublishDate = new DateTime(2011, 1, 6)
                               };

            var user = new User
                           {
                               Username = "Steve",
                               Password = "password"
                           };

            user.AddBlogPost(post1);
            user.AddBlogPost(post2);

            blog1.AddPost(post1);
            blog2.AddPost(post2);

            post1.AddComment(comment1);
            post1.AddComment(comment2);
            post2.AddComment(comment3);

            Repository.Insert(user);
            Repository.Insert(blog1, blog2);
            Repository.Insert(post1, post2);
            Repository.Insert(comment1, comment2, comment3);

            var actualPosts = Repository.Find<BlogPost>()
                .OrderBy(x => x.Title)
                .Join<BlogPost, Comment>(x => x.Comments, x => x.BlogPost)
                .ExecuteList();

            var firstActualPost1 = Repository.Find<BlogPost>()
                .Join<Blog, BlogPost>(x => x.Posts, x => x.Blog)
                .Where(x => x.Id == post1.Id)
                .Execute();

            var secondActualPost1 = Repository.Find<BlogPost>()
                .Join<User, BlogPost>(x => x.BlogPosts, x => x.Author)
                .Where(x => x.Id == post1.Id)
                .Execute();

            // Set up the original entities according to the expected result
            post2.Author = null;
            post2.Blog = null;

            Assert.AreEqual(2, actualPosts.Count);

            Assert.AreEqual(post1, actualPosts[0]);
            Assert.AreEqual(post2, actualPosts[1]);

            Assert.AreSame(firstActualPost1, actualPosts[0]);
            Assert.AreSame(secondActualPost1, actualPosts[0]);
        }

        [Test]
        public void Not_equals_operator_can_be_used_to_filter_result()
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

            Repository.Insert(movie1, movie2, movie3);

            var allMovies = Repository.Find<Movie>().Where(x => x.Title != "Movie 2")
                .OrderBy(x => x.Title)
                .ExecuteList();

            Assert.AreEqual(2, allMovies.Count);
            Assert.AreEqual(allMovies[0].Id, movie1.Id);
            Assert.AreEqual(allMovies[1].Id, movie3.Id);
        }

        [Test]
        public void Not_operator_can_be_used_to_filter_result()
        {
            var movie1 = new Movie
                             {
                                 Title = "Movie 1",
                                 ReleaseDate = new DateTime(2012, 1, 2)
                             };

            var movie2 = new Movie
                             {
                                 Title = "Movie 2",
                                 ReleaseDate = new DateTime(2012, 1, 3)
                             };

            var movie2_2 = new Movie
                               {
                                   Title = "Movie 2",
                                   ReleaseDate = new DateTime(2012, 1, 2)
                               };

            var movie3 = new Movie
                             {
                                 Title = "Movie 3",
                                 ReleaseDate = new DateTime(2012, 1, 3)
                             };

            Repository.Insert(movie1, movie2, movie2_2, movie3);

            var allMovies = Repository.Find<Movie>()
                .Where(x => !(x.Title == "Movie 2" && x.ReleaseDate == new DateTime(2012, 1, 2)))
                .OrderBy(x => x.Title)
                .ExecuteList();

            Assert.AreEqual(3, allMovies.Count);
            Assert.AreEqual(allMovies[0].Id, movie1.Id);
            Assert.AreEqual(allMovies[1].Id, movie2.Id);
            Assert.AreEqual(allMovies[2].Id, movie3.Id);
        }

        [Test]
        public void Enum_values_are_stored_as_integers()
        {
            var movie = new Movie
                            {
                                Title = "Movie 1",
                                ReleaseDate = new DateTime(2012, 1, 2),
                                Genre = MovieGenre.Comedy
                            };

            Repository.Insert(movie);

            var actualMovie = Repository.Find<Movie>()
                .Where(x => x.Id == movie.Id)
                .Execute();

            movie.Genre = MovieGenre.SciFi;

            Repository.Update(movie);

            var actualMovieAfterFirstUpdate = Repository.Find<Movie>()
                .Where(x => x.Id == movie.Id)
                .Execute();

            Repository.Update<Movie>()
                .Set(x => x.Genre, MovieGenre.Drama)
                .Where(x => x.Genre == MovieGenre.SciFi)
                .Execute();

            var actualMovieAfterSecondUpdate = Repository.Find<Movie>()
                .Where(x => x.Id == movie.Id)
                .Execute();

            Repository.Delete<Movie>().Where(x => x.Genre == MovieGenre.Drama).Execute();

            var actualMovieAfterDelete = Repository.Find<Movie>()
                .Where(x => x.Id == movie.Id)
                .Execute();

            Assert.AreEqual(MovieGenre.Comedy, actualMovie.Genre);
            Assert.AreEqual(MovieGenre.SciFi, actualMovieAfterFirstUpdate.Genre);
            Assert.AreEqual(MovieGenre.Drama, actualMovieAfterSecondUpdate.Genre);
            Assert.IsNull(actualMovieAfterDelete);
        }

        [Ignore("Not implemented yet")]
        [Test]
        public void Entity_can_be_joined_to_itself()
        {
            var company = new Company
                              {
                                  Name = "Company 1"
                              };

            var employee1 = new Employee
                                {
                                    LastName = "Employee 1"
                                };

            var employee2 = new Employee
                                {
                                    LastName = "Employee 2"
                                };

            var employee3 = new Employee
                                {
                                    LastName = "Employee 3"
                                };

            var manager1 = new Employee
                               {
                                   LastName = "Manager"
                               };

            var manager2 = new Employee
                               {
                                   LastName = "Manager 2"
                               };

            Repository.Insert(company);

            company.AddEmployee(employee1);
            company.AddEmployee(employee2);
            company.AddEmployee(employee3);
            company.AddEmployee(manager1);
            company.AddEmployee(manager2);

            Repository.Insert(manager1, manager2);

            // Add the subordinates after the managers have been saved so that the
            // ManagerIds are set to the correct values
            manager1.AddSubordinate(employee1);
            manager1.AddSubordinate(employee2);
            manager2.AddSubordinate(employee3);

            Repository.Insert(employee1, employee2, employee3);

            var actualManager1 = Repository.Find<Employee>().Where(x => x.Id == manager1.Id)
                .Join<Employee, Employee>(x => x.Subordinates, x => x.Manager)
                .Execute();

            var actualEmployees = Repository.Find<Employee>()
                .Join<Employee, Employee>(x => x.Subordinates, x => x.Manager)
                .OrderBy(x => x.LastName)
                .ExecuteList();

            Assert.AreEqual(manager1, actualManager1);
            Assert.AreEqual(employee1, actualEmployees[0]);
            Assert.AreEqual(employee2, actualEmployees[1]);
            Assert.AreEqual(employee3, actualEmployees[2]);
        }

        [Test]
        public void Entity_without_primary_key_can_be_written_and_read_back_again()
        {
            var @event = new Event { AggregateId = Guid.NewGuid(), PublishDate = new DateTime(2012, 1, 2, 3, 4, 5), Data = "data" };

            Repository.Insert(@event);

            var actualEvent = Repository.Find<Event>().Where(x => x.AggregateId == @event.AggregateId).Execute();

            Assert.AreEqual(actualEvent, @event);
        }

        [Test]
        public void Setting_the_convention_on_one_repository_instance_does_not_change_the_convention_of_another_repository_instance()
        {
            var bookRepository = new Repository { Convention = new BookConvention() };
            var blogRepository = new Repository { Convention = new BlogConvention() };

            var book = new Book
                           {
                               Isbn = "1",
                               AuthorName = "Author Name",
                               Title = "Title 1",
                               PageCount = 100,
                           };

            var user = new User
                           {
                               Id = Guid.NewGuid(),
                               Username = "a username",
                               Password = "a password"
                           };

            var blog = new Blog { Name = "Blog" };
            var post = new BlogPost
                           {
                               Title = "Blog post 1",
                               Content = "Post 1 content",
                               PublishDate = new DateTime(2011, 1, 1),
                           };
            
            blog.AddPost(post);

            blogRepository.Insert(blog);
            blogRepository.Insert(post);
            bookRepository.Insert(book);
            Repository.Insert(user);

            var actualUser = Repository.Find<User>().Where(x => x.Id == user.Id).Execute();
            var actualBook = bookRepository.Find<Book>().Where(x => x.Isbn == book.Isbn).Execute();
            var actualPost = blogRepository.Find<BlogPost>().Where(x => x.Id == post.Id).Execute();

            Assert.AreEqual(book, actualBook);
            Assert.AreEqual(post.Id, actualPost.Id); // Only check id to avoid equals comparison of Blog
            Assert.AreEqual(user, actualUser);
        }
    }
}
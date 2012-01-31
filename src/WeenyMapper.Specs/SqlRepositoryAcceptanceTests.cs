using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs
{
    public class SqlRepositoryAcceptanceTests : AcceptanceSpecsBase
    {
        /* Requirements for running these tests:
           SQL Server Express instance with a database called WeenyMapper
          
           The database should be created from the script file at SqlScripts/CreateTestDatabase.sql */

        protected override void PerformSetUp()
        {
            Repository.DatabaseProvider = new SqlServerDatabaseProvider();
            DeleteAllExistingTestData();
        }

        [Test]
        public void Single_entity_can_be_read_by_custom_sql_query()
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

            Repository.Insert(book1, book2, book3, book4);

            var sqlCommand = new SqlCommand("select [c_ISBN], [c_TITLE], [c_AUTHORNAME], [c_PAGECOUNT] from [t_Books] where [c_ISBN] = @Isbn");
            sqlCommand.Parameters.Add(new SqlParameter("Isbn", book2.Isbn));

            var readBook2 = Repository.FindBySql<Book>(sqlCommand).Execute();

            Assert.AreEqual(book2, readBook2);
        }

        [Test]
        public void Multiple_entities_can_be_read_by_custom_sql_query()
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

            Repository.Insert(book1, book2, book3, book4);

            var sqlCommand = new SqlCommand("select [c_ISBN], [c_TITLE] from [t_Books] where [c_AUTHORNAME] = @AuthorName");
            sqlCommand.Parameters.Add(new SqlParameter("AuthorName", "Author Name 2"));

            var readBooks = Repository.FindBySql<Book>(sqlCommand).ExecuteList()
                .OrderBy(x => x.Title).ToList();

            Assert.AreEqual(2, readBooks.Count);
            Assert.AreEqual("2", readBooks[0].Isbn);
            Assert.AreEqual("Book title 2", readBooks[0].Title);
            Assert.AreEqual("3", readBooks[1].Isbn);
            Assert.AreEqual("Book title 3", readBooks[1].Title);
        }

        [Test]
        public void Scalar_value_can_be_read_by_custom_sql_query()
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
                    PageCount = 321
                };

            Repository.Insert(book1, book2);

            var sqlCommand = new SqlCommand("select [c_PAGECOUNT] from [t_Books] where [c_ISBN] = @Isbn");
            sqlCommand.Parameters.Add(new SqlParameter("Isbn", "2"));

            var actualPageCount = Repository.FindBySql<Book>(sqlCommand).ExecuteScalar<int>();

            Assert.AreEqual(321, actualPageCount);
        }

        [Test]
        public void Scalar_values_from_multiple_entities_can_be_read_by_custom_sql_query()
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

            Repository.Insert(book1, book2, book3, book4);

            var sqlCommand = new SqlCommand("select [c_ISBN] from [t_Books] where [c_AUTHORNAME] = @AuthorName");
            sqlCommand.Parameters.Add(new SqlParameter("AuthorName", "Author Name 2"));

            var readIsbnValues = Repository.FindBySql<Book>(sqlCommand).ExecuteScalarList<string>()
                .OrderBy(x => x).ToList();

            Assert.AreEqual(2, readIsbnValues.Count);
            Assert.AreEqual("2", readIsbnValues[0]);
            Assert.AreEqual("3", readIsbnValues[1]);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Builders;
using WeenyMapper.Logging;

namespace WeenyMapper.Docs.Samples
{
    [TestFixture]
    public class Examples
    {
        private static Repository repository;
        private Movie[] _movies;
        private Book[] _books;

        [SetUp]
        public void SetUp()
        {
            Repository.DefaultConnectionString = ConnectionString.CreateWithTrustedConnection(".\\SQLEXPRESS", "weenymapper_docs");

            Repository.DefaultConvention = new Convention();

            repository = new Repository();
            repository.EnableSqlConsoleLogging();

            DeleteTestData();

            CreateTestData();
        }

        [Test]
        public void Reading_scalar_values_from_custom_SQL_queries()
        {
            var movieId = _movies.First().Id;

            string title = repository.Find<Movie>()
                .Where(x => x.Id == movieId)
                .Select(x => x.Title)
                .ExecuteScalar<string>();

            IList<string> titles = repository.Find<Movie>()
                .Select(x => x.Title)
                .OrderBy(x => x.Title)
                .ExecuteScalarList<string>();

            Assert.AreEqual("Title 1", title);
            Assert.AreEqual(_movies.Count(), titles.Count);
            Assert.AreEqual("Title 1", titles.First());
        }

        [Test]
        public void Selecting_scalar_values_from_custom_SQL_query_with_parameter()
        {
            var command = new SqlCommand(@"SELECT [Title] FROM [Book] WHERE [AverageRating] >= @rating");
            command.Parameters.Add(new SqlParameter("rating", 3));

            var titles = repository.FindBySql<Book>(command).ExecuteScalarList<string>();
            Assert.AreEqual(4, titles.Count);
        }

        [Test]
        public void Selecting_entire_entities_from_a_custom_SQL_query()
        {
            var command = new SqlCommand(@"SELECT [ISBN], [Author], [Title], [AverageRating] FROM [Book] WHERE [AverageRating] = (SELECT MAX([AverageRating]) FROM [Book])");

            var bestRatedBooks = repository.FindBySql<Book>(command).ExecuteList();
            Assert.AreEqual(1, bestRatedBooks.Count);
        }

        [Test]
        public void Selectig_entire_entities_from_SQL_query_with_parameter()
        {
            var command = new SqlCommand(@"SELECT [ISBN], [Author], [Title], [AverageRating] FROM [Book] WHERE [AverageRating] >= @rating");
            command.Parameters.Add(new SqlParameter("rating", 3));

            var books = repository.FindBySql<Book>(command).ExecuteList();

            Assert.AreEqual(4, books.Count);
        }

        [Test]
        public void Selecting_a_subset_of_the_columns()
        {
            var books = repository.Find<Book>()
                .Select(x => x.ISBN)
                .Select(x => x.Author)
                .ExecuteList();

            Assert.IsNull(books.First().Title);
        }

        [Test]
        public void Selecting_all_of_the_columns()
        {
            var books = repository.Find<Book>()
                .ExecuteList();
            
            Assert.IsNotNull(books.First().Title);
        }

        [Test]
        public void Selecting_a_single_value_as_a_scalar_list()
        {
            var titles = repository.Find<Book>().Select(x => x.Title).ExecuteScalarList<string>();
        }

        [Test]
        public void Selecting_a_mini_version_of_an_entity()
        {
            var miniBooks = repository.Find<MiniBook>()
                .ExecuteList();

            Assert.IsNotNull(miniBooks.First().Title);
        }

        [Test]
        public void Enabling_sql_logging()
        {
            Repository.SqlLogger = new ConsoleSqlCommandLogger();

            repository.EnableSqlConsoleLogging();

            Assert.IsInstanceOf<TraceSqlCommandLogger>(Repository.SqlLogger);
        }


        private void CreateTestData()
        {
            _movies = new[]
                          {
                              new Movie { Director = "Some director", Title = "Title 1" },
                              new Movie { Director = "Some director", Title = "Title 2" },
                              new Movie { Director = "Some director", Title = "Title 3" },
                              new Movie { Director = "Some other director", Title = "Title 4" },
                              new Movie { Director = "Some other director", Title = "Title 5" },
                          };

            _books = new[]
                         {
                             new Book { ISBN = "ISBN1", Author = "Some author", Title = "Title 1", AverageRating = 1 },
                             new Book { ISBN = "ISBN2", Author = "Some author", Title = "Title 2", AverageRating = 1 },
                             new Book { ISBN = "ISBN3", Author = "Some author", Title = "Title 3", AverageRating = 3 },
                             new Book { ISBN = "ISBN4", Author = "Some other author", Title = "Title 4", AverageRating = 4 },
                             new Book { ISBN = "ISBN5", Author = "Some other author", Title = "Title 5", AverageRating = 5 },
                             new Book { ISBN = "ISBN6", Author = "Some other author", Title = "Title 6", AverageRating = 2 },
                             new Book { ISBN = "ISBN7", Author = "Some other author", Title = "Title 7", AverageRating = 1 },
                             new Book { ISBN = "ISBN8", Author = "Some other author", Title = "Title 9", AverageRating = 3 },
                             new Book { ISBN = "ISBN9", Author = "Some other author", Title = "Title 10", AverageRating = 2 },
                         };

            repository.InsertCollection(_movies);
            repository.InsertCollection(_books);
        }

        private void DeleteTestData()
        {
            repository.Delete<Movie>().Execute();
            repository.Delete<Book>().Execute();
        }
    }
}
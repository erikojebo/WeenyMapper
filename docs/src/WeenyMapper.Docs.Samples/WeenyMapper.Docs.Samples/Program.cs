using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using NUnit.Framework;
using WeenyMapper.Builders;
using System.Linq;
using WeenyMapper.Logging;

namespace WeenyMapper.Docs.Samples
{
    internal class Program
    {
        private static Dictionary<string, Action> _options;
        private static Repository repository;

        static Program()
        {
            repository = CreateRepository();
        }

        private static void Main(string[] args)
        {
            Repository.DefaultConnectionString = ConnectionString.CreateWithTrustedConnection(".\\SQLEXPRESS", "weenymapper_docs");

            Repository.DefaultConvention = new Convention();
            
            repository = new Repository();
            Repository.SqlLogger = new TraceSqlCommandLogger();

            var exit = false;

            while (!exit)
            {
                _options = new Dictionary<string, Action>
                               {
                                   { "Create books", () => CreateTestData(CreateBook, x => x.ISBN) },
                                   { "Create movies", () => CreateTestData(CreateMovie, x => x.Id) },
                                   { "List all books", ListAllBooks },
                                   { "List books using paging query", ListBooksByPage },
                                   { "List movies using paging query", ListMovies },                                   
                                   { "Find books by substring of title", FindBooksByTitleSubstring },
                                   { "Delete books", Delete<Book> },
                                   { "Delete movies", Delete<Movie> },
                                   { "Delete first book with a given title", DeleteFirstBookWithTitle },
                                   { "Delete all books with a given title", DeleteAllBooksWithTitle },
                                   { "Update first book with a given title", UpdateBook },
                                   { "Update titles for all books with a given title", UpdateAllBookTitles },
                                   { "Exit", () => exit = true }
                               };

                for (int i = 0; i < _options.Count; i++)
                {
                    var kv = _options.ElementAt(i);
                    Console.WriteLine(string.Format("{0}. {1}", i + 1, kv.Key));
                }

                var option = ReadInt();
                _options.ElementAt(option - 1).Value.Invoke();

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static void DeleteAllBooksWithTitle()
        {
            Console.Write("Title to delete: ");
            var title = Console.ReadLine();
            Console.WriteLine("Deleting the fist book with the title: " + title);
            repository.Delete<Book>().Where(x => x.Title == title).Execute();
        }

        [Test]
        public void deletemany()
        {
            repository.Delete<Book>().Where(x => x.Title == "Some title").Execute();            
        }

        private static void DeleteFirstBookWithTitle()
        {
            Console.Write("Title to delete: ");
            var title = Console.ReadLine();
            Console.WriteLine("Deleting all books with the title: " + title);
            repository.Delete<Book>().Where(x => x.Title == title).Execute();
        }

        private static void UpdateBook()
        {
            Console.Write("Title for book to update: ");
            var title = Console.ReadLine();

            Console.Write("New title: ");
            var newTitle = Console.ReadLine();

            Console.WriteLine("Updating all first book with the given title...");
            var book = repository.Find<Book>().Where(x => x.Title == title).Top(1).Execute();

            book.Title = newTitle;

            Console.WriteLine("Updating the first book with the given title...");

            repository.Update(book);
        }

        private static void UpdateAllBookTitles()
        {
            Console.Write("Title for books to update: ");
            var title = Console.ReadLine();

            Console.Write("New title: ");
            var newTitle = Console.ReadLine();

            Console.WriteLine("Updating all books with the given title...");
            repository.Update<Book>().Where(x => x.Title == title).Set(x => x.Title, newTitle).Execute();
        }

        private static int ReadInt()
        {
            return int.Parse(Console.ReadLine());
        }

        private static void ListAllBooks()
        {
            var books = repository.Find<Book>()
                .OrderBy(x => x.Title)
                .ExecuteList();

            foreach (var book in books)
            {
                Console.WriteLine(book.Title);
            }
        }

        private static void FindBooksByTitleSubstring()
        {
            Console.Write("Enter the substring to search for: ");

            var substring = Console.ReadLine();

            var books = repository.Find<Book>()
                .Where(x => x.Title.Contains(substring))
                .OrderBy(x => x.Title)
                .ExecuteList();

            foreach (var book in books)
            {
                Console.WriteLine(book.Title);
            }
        }

        private static void ListBooksByPage()
        {
            var page = GetPage();

            var books = repository.Find<Book>()
                .Page(page.Number - 1, page.Size)
                .OrderBy(x => x.Title)
                .ExecuteList();

            foreach (var book in books)
            {
                Console.WriteLine(book.Title);
            }
        }

        private static void ListMovies()
        {
            var page = GetPage();

            var books = repository.Find<Movie>()
                .Page(page.Number - 1, page.Size)
                .OrderBy(x => x.Title)
                .ExecuteList();

            foreach (var book in books)
            {
                Console.WriteLine(book.Title);
            }
        }

        private class Page
        {
            public Page(int size, int number)
            {
                Size = size;
                Number = number;
            }

            public readonly int Size;
            public readonly int Number;
        }

        private static Page GetPage()
        {
            Console.Write("Page size: ");
            var pageSize = ReadInt();

            Console.Write("Page number: ");
            var pageNumber = ReadInt();

            return new Page(pageSize, pageNumber);
        }

        public static Movie CreateMovie(int x)
        {
            return new Movie
                       {
                           Title = "Movie Title " + x,
                           Director = "Movie director " + x,
                       };
        }

        public static Book CreateBook(int x)
        {
            return  new Book()
            {
                Author = "Author " + x,
                Title = "Title " + x,
                ISBN = "ISBN" + x
            };
        }

        private static void CreateTestData<T>(Func<int, T> selector, Func<T, object> idSelector)
        {
            Console.Write("Number of items to create: ");
            var itemCount = ReadInt();

            var entities = Enumerable.Range(1, itemCount).Select(selector).ToList();

            repository.InsertCollection(entities);

            var ids = entities.Select(idSelector);
            Console.WriteLine("Ids after insert: " + string.Join(", ", ids));
        }

        private static void Delete<T>()
        {
            repository.Delete<T>().Execute();
        }

        [Test]
        public void Select_all()
        {
            var repo = CreateRepository();
            repo.Find<Book>().ExecuteList();
        }

        [Test]
        public void Select_one()
        {
            var entity = new Movie() { Title = "title", Director = "director" };
            var repository = CreateRepository();

            repository.Insert(entity);

            var movie = repository.Find<Movie>().Where(x => x.Id == entity.Id).Execute();

            Assert.IsNotNull(movie);

            var nullBook = repository.Find<Book>().Where(x => x.ISBN == "non existing").Execute();
            
            Assert.IsNull(nullBook);
        }

        [Test]
        public void combining_operators_and_parens()
        {
            var moviesToSave = new[]
                             {
                                 new Movie() { Director = "Some director", Title = "Title 1" },
                                 new Movie() { Director = "Some director", Title = "Title 2" },
                                 new Movie() { Director = "Some director", Title = "Title 5" },
                                 new Movie() { Director = "Some director", Title = "Title 6" },
                                 new Movie() { Director = "Some other director", Title = "Title 1" },
                                 new Movie() { Director = "Some other director", Title = "Title 2" },
                                 new Movie() { Director = "Some other director", Title = "Title 3" },
                                 new Movie() { Director = "Some other director", Title = "Title 4" },
                             };

            var repository = CreateRepository();
            repository.Delete<Movie>().Execute();
            repository.InsertCollection(moviesToSave);

            var movies = repository.Find<Movie>()
                .Where(x => x.Director == "Some director" && (x.Title == "Title 1" || x.Title == "Title 2"))
                .ExecuteList();

            Assert.AreEqual(2, movies.Count);
            Assert.That(movies.Any(x => x.Title == "Title 1" && x.Director == "Some director"));
            
            Assert.That(movies.Any(x => x.Title == "Title 2"));
            Assert.That(!movies.Any(x => x.Title == "Title 3"));
        }

        [Test]
        public void order_by_desc()
        {
            var moviesToSave = new[]
                             {
                                 new Movie() { Director = "Some director", Title = "Title 1" },
                                 new Movie() { Director = "Some director", Title = "Title 2" },
                                 new Movie() { Director = "Some director", Title = "Title 5" },
                                 new Movie() { Director = "Some director", Title = "Title 6" },
                                 new Movie() { Director = "Some other director", Title = "Title 1" },
                                 new Movie() { Director = "Some other director", Title = "Title 2" },
                                 new Movie() { Director = "Some other director", Title = "Title 3" },
                                 new Movie() { Director = "Some other director", Title = "Title 4" },
                             };

            var repository = CreateRepository();
            repository.Delete<Movie>().Execute();
            repository.InsertCollection(moviesToSave);

            var movies = repository.Find<Movie>()
                .OrderBy(x => x.Director)
                .OrderByDescending(x => x.Title)
                .ExecuteList();

            var movies_ = repository.Find<Movie>()
                .Where(x => x.Director == "Some director")
                .OrderBy(x => x.Title)
                .ExecuteList();

        }

        [Test]
        public void Inserts()
        {
            var repository = CreateRepository();

            var movie1 = new Movie
            {
                Title = "Movie Title 1",
                Director = "Movie director 1",
            };

            var movie2 = new Movie
            {
                Title = "Movie Title 2",
                Director = "Movie director 2",
            };

            var movies = new[] { movie1, movie2 };

            // You can insert a single object
            repository.Insert(movie1);

            // ... or a fixed number of objects
            repository.Insert(movie1, movie2);

            // ... or a collection of objects
            repository.InsertCollection(movies);

        }

        [Test]
        public void paging()
        {
            var repository = CreateRepository();

var pageIndex = 5;
var pageSize = 10;

var movies = repository
    .Find<Movie>()
    .OrderBy(x => x.Title)
    .Page(pageIndex, pageSize)
    .ExecuteList();

        }

        [Test]
        public void Top()
        {
            var movies = repository.Find<Movie>().Top(10).ExecuteList();

        }

        [Test]
        public void Count()
        {
            var count = repository.Count<Movie>().Where(x => x.Director == "Some director").Execute();

        }

        [Test]
        public void delete_one()
        {
            repository.Delete<Movie>().Execute();
            var movie = new Movie() { Director = "some director", Title = "some title" };
            repository.Insert(movie);

            var movieId = movie.Id;

            var existingMovie = repository.Find<Movie>().Where(x => x.Id == movieId);

            Assert.IsNotNull(existingMovie.Execute());

            repository.Delete(movie);

            Assert.IsNull(repository.Find<Movie>().Where(x => x.Id == movie.Id).Execute());
        }

        private static Repository CreateRepository()
        {
            Repository.DefaultConnectionString = ConnectionString.CreateWithTrustedConnection(".\\SQLEXPRESS", "weenymapper_docs");

            Repository.DefaultConvention = new Convention();

            repository = new Repository();
            repository.EnableSqlConsoleLogging();

            return repository;
        }


        [Test]
        public void custom_sql()
        {
            var selectAllCommand = new SqlCommand("SELECT UPPER([Title]) FROM Movie");
        
            var selectOneCommand = new SqlCommand("SELECT UPPER([Title]) FROM Movie WHERE [Id] = @Id");
            selectOneCommand.Parameters.Add(new SqlParameter("Id", 64));

            string title = repository.FindBySql<Movie>(selectOneCommand).ExecuteScalar<string>();
            IList<string> titles = repository.FindBySql<Movie>(selectAllCommand).ExecuteScalarList<string>();


        }



        [Test]
        public void update_one()
        {
            repository.Insert(new Movie(){Director = "Some director", Title = "Some title"});

            var movie = repository.Find<Movie>().Execute();
            
            movie.Title = "Some new title";

            repository.Update(movie);

            repository.Update<Movie>()
                .Set(x => x.Title, "Some new title")
                .Set(x => x.Director, "Some new director")
                .Where(x => x.Director == "Some director")
                .Execute();
        }

        [Test]
        public void logger()
        {
            Repository.SqlLogger = new TraceSqlCommandLogger();
            
        }
    }

    // hai...
public class CustomLogger : ISqlCommandLogger
{
    public void Log(DbCommand command)
    {
        var sql = command.CommandText;
        var parameters = command.Parameters;

        // Do something smart...
    }
}

public class EasierCustomLogger : SqlCommandLoggerBase
{
    protected override void OutputLogEntry(string logEntry)
    {
        throw new NotImplementedException();
    }
}
}
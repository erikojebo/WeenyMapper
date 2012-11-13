WeenyMapper supports many of the most common types of queries, but in most applications there is some query that is a bit out of the ordinary. Instead of trying to support all possible kinds of queries in WeenyMapper, it allows you to run you crazy queries as ordinary SQL Commands, and then helps you with the mapping of the result set back to your entities.

Here is an example of a custom SQL query which uses WeenyMapper to map the result back to Book entities:

```c#
var command = new SqlCommand(@"SELECT [ISBN], [Author], [Title], [AverageRating] FROM [Book] WHERE [AverageRating] = (SELECT MAX([AverageRating]) FROM [Book])");

var bestRatedBooks = repository.FindBySql<Book>(command).ExecuteList();
```

If you want to run parameterized queries, you can go right ahead and do it the same old way as in any other ADO.NET query.
```c#
var command = new SqlCommand(@"SELECT [ISBN], [Author], [Title], [AverageRating] FROM [Book] WHERE [AverageRating] >= @rating");
command.Parameters.Add(new SqlParameter("rating", 3));

var books = repository.FindBySql<Book>(command).ExecuteList();
```

You don't have to read entire entities. You could just as well execute a query that returns a single value per row.

```c#
var command = new SqlCommand(@"SELECT [Title] FROM [Book] WHERE [AverageRating] >= @rating");
command.Parameters.Add(new SqlParameter("rating", 3));

var titles = repository.FindBySql<Book>(command).ExecuteScalarList<string>();
```

You can read more about executing scalar queries with WeenyMapper [here](wiki/Scalar-Queries).

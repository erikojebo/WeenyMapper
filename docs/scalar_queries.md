# Scalar Queries

Not all queries need to fetch entire objects. Oftentimes all you need is one particular value. This can be done in WeenyMapper by executing the query using the `ExecuteScalar<T>` or `ExecuteScalarList<T>` methods on the query.

To be able to use those methods the query needs to return only one value per row. This can be achieved in a few different ways. The most common scenario is that you execute an ordinary query but explicitly specify that only one of the columns should be retrieved.

```c#
var movieId = 64;
string title = repository.Find<Movie>()
    .Where(x => x.Id == movieId)
    .Select(x => x.Title)
    .ExecuteScalar<string>();

IList<string> titles = repository.Find<Movie>()
    .Select(x => x.Title)
    .ExecuteScalarList<string>();
```

Another common scenario is that you execute a custom SQL query through WeenyMapper which returns only one value per row.

```c#
// Custom SQL query returning many rows, with one value for each row:
var selectAllCommand = new SqlCommand("SELECT UPPER([Title]) FROM Movie");

IList<string> titles = repository.FindBySql<Movie>(selectAllCommand).ExecuteScalarList<string>();

// Custom SQL query returning a single row with a single value:
var selectOneCommand = new SqlCommand("SELECT UPPER([Title]) FROM Movie WHERE [Id] = @Id");
selectOneCommand.Parameters.Add(new SqlParameter("Id", 64));

string title = repository.FindBySql<Movie>(selectOneCommand).ExecuteScalar<string>();
```

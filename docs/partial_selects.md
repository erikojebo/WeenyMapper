In your every day app building many of the queries you run don't actually need to fetch all columns. Perhaps you really just need one or a couple of the columns. This can easily be achieved with WeenyMapper's partial select queries.

To specify which columns you want WeenyMapper to fetch, you use the Select method. Each call to Select points out one property which should be loaded. You can chain an arbitrary number of calls to select.

```c#
var books = repository.Find<Book>()
    .Select(x => x.ISBN)
    .Select(x => x.Author)
    .ExecuteList();

// Executed SQL:
// SELECT [ISBN], [Author] FROM [Book] ()
```

If you only select one of the columns you probably don't want to get entities back from the query. You probably want a list of the selected data type. You can do this using [scalar queries](Scalar-Queries).

```c#
var titles = repository.Find<Book>().Select(x => x.Title).ExecuteScalarList<string>();

// Executed SQL:
// SELECT [Title] FROM [Book] ()
```

If you don't explicitly specify which columns to select, WeenyMapper automatically selects all the columns which have a matching property in the type of entity that you want to read the data into.

```c#
var books = repository.Find<Book>()
    .ExecuteList();

// Executed SQL:
// SELECT [ISBN], [Author], [Title], [AverageRating] FROM [Book] ()
```

Let's say that you have a smaller class that only contains a subset of the properties of the Books class.

```c#
public class MiniBook
{
    public string Title { get; set; }
    public string Author { get; set; }
}
```

If you use this class to fetch data from the Books table (requires some [convention](Conventions) tweaking) WeenyMapper will automatically only select the Title and Author columns, since those are the only ones that have matching properties in the resulting entity class.

Here is a sample convention:

```c#
public class BookConvention : DefaultConvention
{
    public override bool IsIdProperty(System.Reflection.PropertyInfo propertyInfo)
    {
        if (propertyInfo.Name == "ISBN")
            return true;

        return base.IsIdProperty(propertyInfo);
    }

    public override string GetTableName(System.Type entityType)
    {
        if (entityType.Name == "MiniBook")
            return "Book";

        return base.GetTableName(entityType);
    }
}
```

And here is how to use that convention and the `MiniBook` class to select a subset of the columns of the Book table.

```c#
repository.Convention = new BookConvention();

var miniBooks = repository.Find<MiniBook>()
    .ExecuteList();

// Executed SQL:
// SELECT [Title], [Author] FROM [Book] ()
```


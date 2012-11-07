```csharp
// If you always want to use the same connection string you can
// specify a default connection string that will be used if no
// other connection string is explicitly specified
Repository.DefaultConnectionString = @"Data source=.\SQLEXPRESS;Initial Catalog=yourdatabase_name;Trusted_Connection=true";  

// The Repository class is the central class in WeenyMapper. 
// Through the repository you make all your queries and commands.
var repository = new Repository();

// If you want to specify another connection string than the
// default one, you can do this on your repository instance
repository.ConnectionString = @"Data source=.\SQLEXPRESS;Initial Catalog=some_other_database_name;Trusted_Connection=true"
```
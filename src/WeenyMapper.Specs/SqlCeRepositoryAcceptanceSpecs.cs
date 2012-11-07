using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using WeenyMapper.Logging;
using WeenyMapper.Sql;
using WeenyMapper.SqlCe;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class SqlCeRepositoryAcceptanceSpecs : RepositoryAcceptanceSpecs
    {
        protected override void PerformSetUp()
        {
            Repository.DatabaseProvider = new SqlCeDatabaseProvider();

            CreateDatabaseFile();
            WriteDatabaseSchema();
        }

        [Test]
        [Ignore("Not supported")]
        public override void Part_of_the_result_can_be_returned_from_find_query_by_specifying_page_number_and_size()
        {
        }

        [Test]
        [Ignore("Not supported")]
        public override void Paging_query_without_explicit_ordering_orders_by_primary_key()
        {
        }

        private void WriteDatabaseSchema()
        {
            var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var resourceName = resourceNames.FirstOrDefault(x => x.Contains("CreateTestDatabase.sql"));

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var sql = reader.ReadToEnd();

                var commandFactory = new SqlCeCommandFactory();

                var commandTexts = sql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));

                var commands = commandTexts.Select(commandFactory.CreateCommand);

                var sqlCommandExecutor = new DbCommandExecutor(new NullSqlCommandLogger(), commandFactory);
                sqlCommandExecutor.ExecuteNonQuery(commands, TestConnectionString);
            }
        }

        private void CreateDatabaseFile()
        {
            if (File.Exists(DatabasePath))
            {
                try
                {
                    File.Delete(DatabasePath);
                }
                catch
                {
                }
            }

            new SqlCeEngine(TestConnectionString).CreateDatabase();
        }

        public override string TestConnectionString
        {
            get { return "DataSource=" + DatabasePath; }
        }

        private string DatabasePath
        {
            get { return Path.Combine(Path.GetTempPath(), "WeenyMapper.sdf"); }
        }
    }
}
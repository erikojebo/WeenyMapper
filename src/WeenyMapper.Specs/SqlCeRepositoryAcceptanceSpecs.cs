using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using WeenyMapper.Specs.SqlScripts;
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
            TestDatabase.Create(TestConnectionString, new SqlCeCommandFactory());
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
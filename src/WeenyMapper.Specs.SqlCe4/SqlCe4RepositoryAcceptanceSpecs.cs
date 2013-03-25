using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Logging;
using WeenyMapper.Specs.SqlScripts;
using WeenyMapper.Sql;
using WeenyMapper.SqlCe4;
using WeenyMapper.SqlCe4.Sql;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class SqlCe4RepositoryAcceptanceSpecs : RepositoryAcceptanceSpecs
    {
        protected override void PerformSetUp()
        {
            Repository.DatabaseProvider = new SqlCe4DatabaseProvider();

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
            TestDatabase.Create(TestConnectionString, new SqlCe4CommandFactory());
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
            get { return Path.Combine(Path.GetTempPath(), "WeenyMapperCE4.sdf"); }
        }
    }
}
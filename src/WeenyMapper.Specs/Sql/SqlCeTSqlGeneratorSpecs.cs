using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class SqlCeTSqlGeneratorSpecs
    {
        private TSqlGenerator _generator;
        private AliasedSqlSubQuery _subQuery;

        [SetUp]
        public void SetUp()
        {
            var sqlServerCommandFactory = new SqlServerCommandFactory(null);

            _generator = new SqlCeTSqlGenerator(sqlServerCommandFactory);

            _subQuery = new AliasedSqlSubQuery();

            _subQuery.ExplicitlySpecifiedColumnsToSelect = new[] { "ColumnName1", "ColumnName2" };
            _subQuery.TableName = "TableName";
            _subQuery.PrimaryKeyColumnName = "IdColumnName";
        }

        [Test]
        public void Insert_command_for_object_with_identity_id_selects_identity_value()
        {
            var propertyValues = new Dictionary<string, object>();

            propertyValues["ColumnName1"] = "value 1";
            propertyValues["ColumnName2"] = "value 2";

            var scalarCommand = _generator.CreateIdentityInsertCommand("TableName", propertyValues);
            var insertCommand = scalarCommand.PreparatoryCommands.FirstOrDefault();
            var selectCommand = scalarCommand.ResultCommand;

            var expectedInsertSql = "INSERT INTO [TableName] ([ColumnName1], [ColumnName2]) VALUES (@ColumnName1, @ColumnName2)";
            var expectedSelectSql = "SELECT CAST(@@IDENTITY AS int)";

            Assert.AreEqual(1, scalarCommand.PreparatoryCommands.Count);

            Assert.AreEqual(expectedInsertSql, insertCommand.CommandText);
            Assert.AreEqual(expectedSelectSql, selectCommand.CommandText);

            Assert.AreEqual(2, insertCommand.Parameters.Count);

            Assert.AreEqual("ColumnName1", insertCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value 1", insertCommand.Parameters[0].Value);

            Assert.AreEqual("ColumnName2", insertCommand.Parameters[1].ParameterName);
            Assert.AreEqual("value 2", insertCommand.Parameters[1].Value);
        }
    }
}
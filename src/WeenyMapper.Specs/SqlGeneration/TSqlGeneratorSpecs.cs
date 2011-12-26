using System.Collections.Generic;
using NUnit.Framework;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper.Specs.SqlGeneration
{
    [TestFixture]
    public class TSqlGeneratorSpecs
    {
        private TSqlGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new TSqlGenerator();
        }

        [Test]
        public void Generating_select_without_constraints_generates_select_star_without_where_clause()
        {
            var constraints = new Dictionary<string, object>();
            var query = _generator.GenerateSelectQuery("TableName", constraints);

            Assert.AreEqual("select * from [TableName]", query.CommandText);
        }

        [Test]
        public void Generating_select_with_single_constraints_generates_select_star_with_parameterized_where_clause()
        {
            var constraints = new Dictionary<string, object>();
            constraints["ColumnName"] = "value";

            var sqlCommand = _generator.GenerateSelectQuery("TableName", constraints);

            Assert.AreEqual("select * from [TableName] where [ColumnName] = @ColumnName", sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value", sqlCommand.Parameters[0].Value);
        }

        [Test]
        public void Generating_select_with_multiple_constraints_generates_select_star_with_where_clause_containing_both_constraints()
        {
            var constraints = new Dictionary<string, object>();
            constraints["ColumnName1"] = "value";
            constraints["ColumnName2"] = 123;

            var sqlCommand = _generator.GenerateSelectQuery("TableName", constraints);

            var expectedQuery = "select * from [TableName] " +
                                "where [ColumnName1] = @ColumnName1 and [ColumnName2] = @ColumnName2";

            Assert.AreEqual(expectedQuery, sqlCommand.CommandText);

            Assert.AreEqual(2, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName1", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value", sqlCommand.Parameters[0].Value);
            Assert.AreEqual("ColumnName2", sqlCommand.Parameters[1].ParameterName);
            Assert.AreEqual(123, sqlCommand.Parameters[1].Value);
        }

        [Test]
        public void Insert_command_for_object_has_column_name_and_parameterized_value_for_each_property()
        {
            var propertyValues = new Dictionary<string, object>();

            propertyValues["ColumnName1"] = "value 1";
            propertyValues["ColumnName2"] = "value 2";

            var sqlCommand = _generator.CreateInsertCommand("TableName", propertyValues);

            Assert.AreEqual("insert into [TableName] ([ColumnName1], [ColumnName2]) values (@ColumnName1, @ColumnName2)", sqlCommand.CommandText);

            Assert.AreEqual(2, sqlCommand.Parameters.Count);

            Assert.AreEqual("ColumnName1", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value 1", sqlCommand.Parameters[0].Value);

            Assert.AreEqual("ColumnName2", sqlCommand.Parameters[1].ParameterName);
            Assert.AreEqual("value 2", sqlCommand.Parameters[1].Value);
        }
    }
}
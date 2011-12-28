using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
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
        public void Generating_select_without_constraints_generates_select_of_escaped_column_names_without_where_clause()
        {
            var columnsToSelect = new[] { "ColumnName1", "ColumnName2" };
            var constraints = new Dictionary<string, object>();
            var query = _generator.GenerateSelectQuery("TableName", columnsToSelect, constraints);

            Assert.AreEqual("select [ColumnName1], [ColumnName2] from [TableName]", query.CommandText);
        }

        [Test]
        public void Generating_select_with_single_constraints_generates_select_with_parameterized_where_clause()
        {
            var columnsToSelect = new[] { "ColumnName" };
            var constraints = new Dictionary<string, object>();
            constraints["ColumnName"] = "value";

            var sqlCommand = _generator.GenerateSelectQuery("TableName", columnsToSelect, constraints);

            Assert.AreEqual("select [ColumnName] from [TableName] where [ColumnName] = @ColumnName", sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value", sqlCommand.Parameters[0].Value);
        }

        [Test]
        public void Generating_select_with_multiple_constraints_generates_select_with_where_clause_containing_both_constraints()
        {
            var columnsToSelect = new[] { "ColumnName1" };
            var constraints = new Dictionary<string, object>();
            constraints["ColumnName1"] = "value";
            constraints["ColumnName2"] = 123;

            var sqlCommand = _generator.GenerateSelectQuery("TableName", columnsToSelect, constraints);

            var expectedQuery = "select [ColumnName1] from [TableName] " +
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

        [Test]
        public void Update_command_for_object_updates_all_properties_constrained_by_id()
        {
            var propertyValues = new Dictionary<string, object>();

            propertyValues["IdColumnName"] = "id value";
            propertyValues["ColumnName1"] = "value 1";
            propertyValues["ColumnName2"] = "value 2";

            var sqlCommand = _generator.CreateUpdateCommand("TableName", "IdColumnName", propertyValues);

            var expectedSql = "update [TableName] set [ColumnName1] = @ColumnName1, [ColumnName2] = @ColumnName2 " +
                              "where [IdColumnName] = @IdColumnNameConstraint";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

            Assert.AreEqual(3, sqlCommand.Parameters.Count);

            var actualParameters = sqlCommand.Parameters.OfType<SqlParameter>().OrderBy(x => x.ParameterName).ToList();

            Assert.AreEqual("ColumnName1", actualParameters[0].ParameterName);
            Assert.AreEqual("value 1", actualParameters[0].Value);

            Assert.AreEqual("ColumnName2", actualParameters[1].ParameterName);
            Assert.AreEqual("value 2", actualParameters[1].Value);

            Assert.AreEqual("IdColumnNameConstraint", actualParameters[2].ParameterName);
            Assert.AreEqual("id value", actualParameters[2].Value);
        }

        [Test]
        public void Update_command_for_mass_update_without_constraints_and_single_setter_creates_parameterized_sql_with_matching_set_clause()
        {
            var columnConstraints = new Dictionary<string, object>();
            var columnSetters = new Dictionary<string, object>();

            columnSetters["ColumnName1"] = "value 1";

            var sqlCommand = _generator.CreateUpdateCommand("TableName", "IdColumnName", columnConstraints, columnSetters);

            var expectedSql = "update [TableName] set [ColumnName1] = @ColumnName1";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);
            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName1", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value 1", sqlCommand.Parameters[0].Value);
        }

        [Test]
        public void Update_command_for_mass_update_with_multiple_constraints_and_setters_creates_parameterized_sql_with_matching_where_and_set_clauses()
        {
            var columnConstraints = new Dictionary<string, object>();
            var columnSetters = new Dictionary<string, object>();

            columnSetters["ColumnName1"] = "value 1";
            columnSetters["ColumnName2"] = "value 2";

            columnConstraints["ColumnName3"] = "value 3";
            columnConstraints["ColumnName4"] = "value 4";

            var sqlCommand = _generator.CreateUpdateCommand("TableName", "IdColumnName", columnConstraints, columnSetters);

            var expectedSql = "update [TableName] set [ColumnName1] = @ColumnName1, [ColumnName2] = @ColumnName2 " +
                              "where [ColumnName3] = @ColumnName3Constraint and [ColumnName4] = @ColumnName4Constraint";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

            Assert.AreEqual(4, sqlCommand.Parameters.Count);

            var actualParameters = sqlCommand.Parameters.OfType<SqlParameter>().OrderBy(x => x.ParameterName).ToList();

            Assert.AreEqual("ColumnName1", actualParameters[0].ParameterName);
            Assert.AreEqual("value 1", actualParameters[0].Value);
            Assert.AreEqual("ColumnName2", actualParameters[1].ParameterName);
            Assert.AreEqual("value 2", actualParameters[1].Value);
            Assert.AreEqual("ColumnName3Constraint", actualParameters[2].ParameterName);
            Assert.AreEqual("value 3", actualParameters[2].Value);
            Assert.AreEqual("ColumnName4Constraint", actualParameters[3].ParameterName);
            Assert.AreEqual("value 4", actualParameters[3].Value);
        }

        [Test]
        public void Delete_command_without_constraints_creates_delete_without_where_clause()
        {
            var columnConstraints = new Dictionary<string, object>();

            var sqlCommand = _generator.CreateDeleteCommand("TableName", columnConstraints);

            Assert.AreEqual("delete from [TableName]", sqlCommand.CommandText);
        }

        [Test]
        public void Delete_command_with_single_constraint_creates_parameterized_delete_with_corresponding_where_clause()
        {
            var columnConstraints = new Dictionary<string, object>();
            columnConstraints["ColumnName1"] = "value 1";

            var sqlCommand = _generator.CreateDeleteCommand("TableName", columnConstraints);
            var actualParameters = sqlCommand.Parameters.OfType<SqlParameter>().OrderBy(x => x.ParameterName).ToList();

            var expectedSql = "delete from [TableName] where [ColumnName1] = @ColumnName1Constraint";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual("value 1", actualParameters[0].Value);
        }

        [Test]
        public void Delete_command_with_multiple_constraints_creates_parameterized_delete_with_corresponding_where_clause()
        {
            var columnConstraints = new Dictionary<string, object>();
            columnConstraints["ColumnName1"] = "value 1";
            columnConstraints["ColumnName2"] = "value 2";

            var sqlCommand = _generator.CreateDeleteCommand("TableName", columnConstraints);
            var actualParameters = sqlCommand.Parameters.OfType<SqlParameter>().OrderBy(x => x.ParameterName).ToList();

            var expectedSql = "delete from [TableName] " +
                              "where [ColumnName1] = @ColumnName1Constraint and [ColumnName2] = @ColumnName2Constraint";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

            Assert.AreEqual(2, sqlCommand.Parameters.Count);

            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual("value 1", actualParameters[0].Value);
            Assert.AreEqual("ColumnName2Constraint", actualParameters[1].ParameterName);
            Assert.AreEqual("value 2", actualParameters[1].Value);
        }
    }
}
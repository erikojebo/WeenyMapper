using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Extensions;
using WeenyMapper.QueryParsing;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class TSqlGeneratorSpecs
    {
        private TSqlGenerator _generator;
        private QuerySpecification _querySpecification;

        [SetUp]
        public void SetUp()
        {
            _generator = new TSqlGenerator();
            _querySpecification = new QuerySpecification();

            _querySpecification.ColumnsToSelect = new[] { "ColumnName1", "ColumnName2" };
            _querySpecification.TableName = "TableName";
        }

        [Test]
        public void Generating_select_without_constraints_generates_select_of_escaped_column_names_without_where_clause()
        {
            _querySpecification.QueryExpression = new RootExpression();

            var query = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("select [ColumnName1], [ColumnName2] from [TableName]", query.CommandText);
        }

        [Test]
        public void Generating_select_with_single_constraints_generates_select_with_parameterized_where_clause()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName" };
            _querySpecification.QueryExpression = new EqualsExpression(new PropertyExpression("ColumnName"), new ValueExpression("value"));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("select [ColumnName] from [TableName] where [ColumnName] = @ColumnNameConstraint", sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnNameConstraint", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value", sqlCommand.Parameters[0].Value);
        }

        [Test]
        public void Generating_select_with_multiple_constraints_generates_select_with_where_clause_containing_both_constraints()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName1" };
            _querySpecification.QueryExpression =
                new RootExpression(
                    new AndExpression(
                        new EqualsExpression(new PropertyExpression("ColumnName1"), new ValueExpression("value")),
                        new EqualsExpression(new PropertyExpression("ColumnName2"), new ValueExpression(123))));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);

            var expectedQuery = "select [ColumnName1] from [TableName] " +
                                "where [ColumnName1] = @ColumnName1Constraint and [ColumnName2] = @ColumnName2Constraint";

            Assert.AreEqual(expectedQuery, sqlCommand.CommandText);

            Assert.AreEqual(2, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName1Constraint", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value", sqlCommand.Parameters[0].Value);
            Assert.AreEqual("ColumnName2Constraint", sqlCommand.Parameters[1].ParameterName);
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
        public void Update_command_for_mass_update_without_constraints_and_single_setter_creates_parameterized_sql_with_matching_set_clause()
        {
            var columnSetters = new Dictionary<string, object>();

            columnSetters["ColumnName1"] = "value 1";

            var queryExpression = QueryExpression.Create();

            var sqlCommand = _generator.CreateUpdateCommand("TableName", "IdColumnName", queryExpression, columnSetters);

            var expectedSql = "update [TableName] set [ColumnName1] = @ColumnName1";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);
            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName1", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value 1", sqlCommand.Parameters[0].Value);
        }

        [Test]
        public void Update_command_for_mass_update_with_query_constraint_and_multiple_setters_creates_parameterized_sql_with_matching_where_and_set_clauses()
        {
            var columnConstraints = new Dictionary<string, object>();
            var columnSetters = new Dictionary<string, object>();

            columnSetters["ColumnName1"] = "value 1";
            columnSetters["ColumnName2"] = "value 2";

            columnConstraints["ColumnName3"] = "value 3";
            columnConstraints["ColumnName4"] = "value 4";

            var expression = new AndExpression(
                new EqualsExpression(new PropertyExpression("ColumnName3"), new ValueExpression("value 3")),
                new EqualsExpression(new PropertyExpression("ColumnName4"), new ValueExpression("value 4")));

            var sqlCommand = _generator.CreateUpdateCommand("TableName", "IdColumnName", expression, columnSetters);

            var expectedSql = "update [TableName] set [ColumnName1] = @ColumnName1, [ColumnName2] = @ColumnName2 " +
                              "where ([ColumnName3] = @ColumnName3Constraint and [ColumnName4] = @ColumnName4Constraint)";

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
            var sqlCommand = _generator.CreateDeleteCommand("TableName", QueryExpression.Create());

            Assert.AreEqual("delete from [TableName]", sqlCommand.CommandText);
        }

        [Test]
        public void Delete_command_with_single_constraint_creates_parameterized_delete_with_corresponding_where_clause()
        {
            var constraintExpression = QueryExpression.Create(new EqualsExpression("ColumnName1", "value 1"));

            var sqlCommand = _generator.CreateDeleteCommand("TableName", constraintExpression);
            var actualParameters = sqlCommand.Parameters.OfType<SqlParameter>().OrderBy(x => x.ParameterName).ToList();

            var expectedSql = "delete from [TableName] where [ColumnName1] = @ColumnName1Constraint";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual("value 1", actualParameters[0].Value);
        }

        [Test]
        public void Delete_command_with_query_constraint_creates_parameterized_delete_with_corresponding_where_clause()
        {
            var expression = new AndExpression(
                new EqualsExpression(new PropertyExpression("ColumnName1"), new ValueExpression("value 1")),
                new EqualsExpression(new PropertyExpression("ColumnName2"), new ValueExpression("value 2")));

            var sqlCommand = _generator.CreateDeleteCommand("TableName", expression);
            var actualParameters = sqlCommand.Parameters.SortByParameterName();

            var expectedSql = "delete from [TableName] " +
                              "where ([ColumnName1] = @ColumnName1Constraint and [ColumnName2] = @ColumnName2Constraint)";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

            Assert.AreEqual(2, sqlCommand.Parameters.Count);

            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual("value 1", actualParameters[0].Value);
            Assert.AreEqual("ColumnName2Constraint", actualParameters[1].ParameterName);
            Assert.AreEqual("value 2", actualParameters[1].Value);
        }

        [Test]
        public void Count_query_without_constraints_creates_sql_count_query_without_where_clause()
        {
            var sqlCommand = _generator.CreateCountCommand("TableName", QueryExpression.Create());

            Assert.AreEqual("select count(*) from [TableName]", sqlCommand.CommandText);
        }

        [Test]
        public void Count_query_with_single_constraint_creates_parameterized_sql_query_with_corresponding_where_clause()
        {
            var queryExpression = QueryExpression.Create(new EqualsExpression("ColumnName1", 1));

            var sqlCommand = _generator.CreateCountCommand("TableName", queryExpression);

            Assert.AreEqual("select count(*) from [TableName] where [ColumnName1] = @ColumnName1Constraint", sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);

            Assert.AreEqual("ColumnName1Constraint", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual(1, sqlCommand.Parameters[0].Value);
        }

        [Test]
        public void Count_query_with_multiple_constraints_creates_parameterized_sql_query_with_corresponding_where_clause()
        {
            var columnConstraints = new Dictionary<string, object>();

            columnConstraints["ColumnName1"] = 1;
            columnConstraints["ColumnName2"] = "2";

            var queryExpression = QueryExpression.Create(new AndExpression(
                new EqualsExpression("ColumnName1", 1),
                new EqualsExpression("ColumnName2", "2")));

            var sqlCommand = _generator.CreateCountCommand("TableName", queryExpression);

            var actualParameters = sqlCommand.Parameters.OfType<SqlParameter>().OrderBy(x => x.ParameterName).ToList();

            var expectedSql = "select count(*) from [TableName] " +
                              "where [ColumnName1] = @ColumnName1Constraint and [ColumnName2] = @ColumnName2Constraint";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

            Assert.AreEqual(2, actualParameters.Count);

            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual(1, actualParameters[0].Value);

            Assert.AreEqual("ColumnName2Constraint", actualParameters[1].ParameterName);
            Assert.AreEqual("2", actualParameters[1].Value);
        }

        [Test]
        public void Expression_with_single_equals_comparison_creates_parameterized_sql_query_with_corresponding_where_clause()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName1", "ColumnName2" };
            _querySpecification.QueryExpression = new EqualsExpression(new PropertyExpression("ColumnName"), new ValueExpression("Value"));
            _querySpecification.TableName = "TableName";

            var expectedSql = "select [ColumnName1], [ColumnName2] from [TableName] where [ColumnName] = @ColumnNameConstraint";

            var command = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(1, command.Parameters.Count);
            Assert.AreEqual("ColumnNameConstraint", command.Parameters[0].ParameterName);
            Assert.AreEqual("Value", command.Parameters[0].Value);
        }

        [Test]
        public void Conjunction_of_equals_expressions_creates_parameterized_sql_query_with_corresponding_where_clause()
        {
            _querySpecification.QueryExpression = new AndExpression(
                new EqualsExpression(new PropertyExpression("ColumnName1"), new ValueExpression(1)),
                new EqualsExpression(new PropertyExpression("ColumnName2"), new ValueExpression(2)));

            var expectedSql = "select [ColumnName1], [ColumnName2] from [TableName] " +
                              "where ([ColumnName1] = @ColumnName1Constraint and [ColumnName2] = @ColumnName2Constraint)";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(2, actualParameters.Count);
            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual(1, actualParameters[0].Value);
            Assert.AreEqual("ColumnName2Constraint", actualParameters[1].ParameterName);
            Assert.AreEqual(2, actualParameters[1].Value);
        }

        [Test]
        public void Disjunction_of_equals_expressions_creates_parameterized_sql_query_with_corresponding_where_clause()
        {
            _querySpecification.QueryExpression = new OrExpression(
                new EqualsExpression(new PropertyExpression("ColumnName1"), new ValueExpression(1)),
                new EqualsExpression(new PropertyExpression("ColumnName2"), new ValueExpression(2)));

            var expectedSql = "select [ColumnName1], [ColumnName2] from [TableName] " +
                              "where ([ColumnName1] = @ColumnName1Constraint or [ColumnName2] = @ColumnName2Constraint)";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(2, actualParameters.Count);
            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual(1, actualParameters[0].Value);
            Assert.AreEqual("ColumnName2Constraint", actualParameters[1].ParameterName);
            Assert.AreEqual(2, actualParameters[1].Value);
        }

        [Test]
        public void Conjunction_of_disjunctions_is_parenthezised_to_ensure_correct_evaluation_order()
        {
            _querySpecification.QueryExpression = new AndExpression(
                new OrExpression(
                    new LessExpression(new PropertyExpression("ColumnName1"), new ValueExpression(1)),
                    new GreaterExpression(new PropertyExpression("ColumnName2"), new ValueExpression(2)),
                    new EqualsExpression(new PropertyExpression("ColumnName2"), new ValueExpression(3))),
                new OrExpression(
                    new GreaterOrEqualExpression(new PropertyExpression("ColumnName1"), new ValueExpression(3)),
                    new LessOrEqualExpression(new PropertyExpression("ColumnName2"), new ValueExpression(4))));

            var expectedSql = "select [ColumnName1], [ColumnName2] from [TableName] " +
                              "where (([ColumnName1] < @ColumnName1Constraint or [ColumnName2] > @ColumnName2Constraint or [ColumnName2] = @ColumnName2Constraint2) and " +
                              "([ColumnName1] >= @ColumnName1Constraint2 or [ColumnName2] <= @ColumnName2Constraint3))";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(5, actualParameters.Count);

            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual(1, actualParameters[0].Value);

            Assert.AreEqual("ColumnName1Constraint2", actualParameters[1].ParameterName);
            Assert.AreEqual(3, actualParameters[1].Value);

            Assert.AreEqual("ColumnName2Constraint", actualParameters[2].ParameterName);
            Assert.AreEqual(2, actualParameters[2].Value);

            Assert.AreEqual("ColumnName2Constraint2", actualParameters[3].ParameterName);
            Assert.AreEqual(3, actualParameters[3].Value);

            Assert.AreEqual("ColumnName2Constraint3", actualParameters[4].ParameterName);
            Assert.AreEqual(4, actualParameters[4].Value);
        }

        [Test]
        public void Query_with_in_expression_is_translated_to_sql_query_with_in_clause()
        {
            var values = new[] { (object)1, 2, 3 };

            _querySpecification.QueryExpression = new InExpression(new PropertyExpression("PropertyName"), new ArrayValueExpression(values));

            var expectedSql = "select [ColumnName1], [ColumnName2] from [TableName] " +
                              "where ([PropertyName] in (@PropertyNameConstraint, @PropertyNameConstraint2, @PropertyNameConstraint3))";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);
            Assert.AreEqual(3, actualParameters.Count);

            Assert.AreEqual("PropertyNameConstraint", actualParameters[0].ParameterName);
            Assert.AreEqual(1, actualParameters[0].Value);

            Assert.AreEqual("PropertyNameConstraint2", actualParameters[1].ParameterName);
            Assert.AreEqual(2, actualParameters[1].Value);

            Assert.AreEqual("PropertyNameConstraint3", actualParameters[2].ParameterName);
            Assert.AreEqual(3, actualParameters[2].Value);
        }
    }
}
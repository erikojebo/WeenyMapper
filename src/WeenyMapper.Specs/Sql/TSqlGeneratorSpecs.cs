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
            _querySpecification.PrimaryKeyColumnName = "IdColumnName";
        }

        [Test]
        public void Generating_select_without_constraints_generates_select_of_escaped_column_names_without_where_clause()
        {
            _querySpecification.QueryExpression = new RootExpression();

            var query = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("SELECT [ColumnName1], [ColumnName2] FROM [TableName]", query.CommandText);
        }

        [Test]
        public void Generating_select_with_single_constraints_generates_select_with_parameterized_where_clause()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName" };
            _querySpecification.QueryExpression = new EqualsExpression(new PropertyExpression("ColumnName"), new ValueExpression("value"));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("SELECT [ColumnName] FROM [TableName] WHERE [ColumnName] = @ColumnNameConstraint", sqlCommand.CommandText);

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

            var expectedQuery = "SELECT [ColumnName1] FROM [TableName] " +
                                "WHERE [ColumnName1] = @ColumnName1Constraint AND [ColumnName2] = @ColumnName2Constraint";

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

            Assert.AreEqual("INSERT INTO [TableName] ([ColumnName1], [ColumnName2]) VALUES (@ColumnName1, @ColumnName2)", sqlCommand.CommandText);

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

            var expectedSql = "UPDATE [TableName] SET [ColumnName1] = @ColumnName1";

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

            var expectedSql = "UPDATE [TableName] SET [ColumnName1] = @ColumnName1, [ColumnName2] = @ColumnName2 " +
                              "WHERE ([ColumnName3] = @ColumnName3Constraint AND [ColumnName4] = @ColumnName4Constraint)";

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

            Assert.AreEqual("DELETE FROM [TableName]", sqlCommand.CommandText);
        }

        [Test]
        public void Delete_command_with_single_constraint_creates_parameterized_delete_with_corresponding_where_clause()
        {
            var constraintExpression = QueryExpression.Create(new EqualsExpression("ColumnName1", "value 1"));

            var sqlCommand = _generator.CreateDeleteCommand("TableName", constraintExpression);
            var actualParameters = sqlCommand.Parameters.OfType<SqlParameter>().OrderBy(x => x.ParameterName).ToList();

            var expectedSql = "DELETE FROM [TableName] WHERE [ColumnName1] = @ColumnName1Constraint";

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

            var expectedSql = "DELETE FROM [TableName] " +
                              "WHERE ([ColumnName1] = @ColumnName1Constraint AND [ColumnName2] = @ColumnName2Constraint)";

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

            Assert.AreEqual("SELECT COUNT(*) FROM [TableName]", sqlCommand.CommandText);
        }

        [Test]
        public void Count_query_with_single_constraint_creates_parameterized_sql_query_with_corresponding_where_clause()
        {
            var queryExpression = QueryExpression.Create(new EqualsExpression("ColumnName1", 1));

            var sqlCommand = _generator.CreateCountCommand("TableName", queryExpression);

            Assert.AreEqual("SELECT COUNT(*) FROM [TableName] WHERE [ColumnName1] = @ColumnName1Constraint", sqlCommand.CommandText);

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

            var expectedSql = "SELECT COUNT(*) FROM [TableName] " +
                              "WHERE [ColumnName1] = @ColumnName1Constraint AND [ColumnName2] = @ColumnName2Constraint";

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

            var expectedSql = "SELECT [ColumnName1], [ColumnName2] FROM [TableName] WHERE [ColumnName] = @ColumnNameConstraint";

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

            var expectedSql = "SELECT [ColumnName1], [ColumnName2] FROM [TableName] " +
                              "WHERE ([ColumnName1] = @ColumnName1Constraint AND [ColumnName2] = @ColumnName2Constraint)";

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

            var expectedSql = "SELECT [ColumnName1], [ColumnName2] FROM [TableName] " +
                              "WHERE ([ColumnName1] = @ColumnName1Constraint OR [ColumnName2] = @ColumnName2Constraint)";

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

            var expectedSql = "SELECT [ColumnName1], [ColumnName2] FROM [TableName] " +
                              "WHERE (([ColumnName1] < @ColumnName1Constraint OR [ColumnName2] > @ColumnName2Constraint OR [ColumnName2] = @ColumnName2Constraint2) AND " +
                              "([ColumnName1] >= @ColumnName1Constraint2 OR [ColumnName2] <= @ColumnName2Constraint3))";

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

            var expectedSql = "SELECT [ColumnName1], [ColumnName2] FROM [TableName] " +
                              "WHERE ([PropertyName] IN (@PropertyNameConstraint, @PropertyNameConstraint2, @PropertyNameConstraint3))";

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

        [Test]
        public void Adding_a_row_count_limit_translates_into_a_top_clause()
        {
            _querySpecification.RowCountLimit = 3;
            _querySpecification.QueryExpression = new EqualsExpression("ColumnName1", 1);

            var expectedSql = "SELECT TOP(@RowCountLimit) [ColumnName1], [ColumnName2] FROM [TableName] " +
                              "WHERE [ColumnName1] = @ColumnName1Constraint";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(2, actualParameters.Count);

            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual(1, actualParameters[0].Value);
            
            Assert.AreEqual("RowCountLimit", actualParameters[1].ParameterName);
            Assert.AreEqual(3, actualParameters[1].Value);
        }

        [Test]
        public void Adding_order_by_statements_adds_corresponding_order_by_clause_to_the_sql_query()
        {
            _querySpecification.QueryExpression = QueryExpression.Create(new EqualsExpression("ColumnName1", "value"));
            _querySpecification.OrderByStatements.Add(new OrderByStatement("ColumnName1", OrderByDirection.Descending));
            _querySpecification.OrderByStatements.Add(new OrderByStatement("ColumnName3"));
            _querySpecification.OrderByStatements.Add(new OrderByStatement("ColumnName2"));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);

            var expectedQuery = "SELECT [ColumnName1], [ColumnName2] FROM [TableName] " +
                                "WHERE [ColumnName1] = @ColumnName1Constraint ORDER BY [ColumnName1] DESC, [ColumnName3], [ColumnName2]";

            Assert.AreEqual(expectedQuery, sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("ColumnName1Constraint", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value", sqlCommand.Parameters[0].Value);
        }

        [Test]
        public void Paging_query_without_constraints_or_ordering_is_translated_to_row_number_query_ordered_by_primary_key()
        {
            _querySpecification.Page = new Page(1, 2);

            var expectedSql = "WITH [CompleteResult] AS (SELECT [ColumnName1], [ColumnName2], ROW_NUMBER() OVER (ORDER BY [IdColumnName]) AS [RowNumber] " +
                              "FROM [TableName]) SELECT [ColumnName1], [ColumnName2] FROM [CompleteResult] WHERE [RowNumber] BETWEEN @LowRowLimit AND @HighRowLimit";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(2, actualParameters.Count);

            Assert.AreEqual("HighRowLimit", actualParameters[0].ParameterName);
            Assert.AreEqual(4, actualParameters[0].Value);

            Assert.AreEqual("LowRowLimit", actualParameters[1].ParameterName);
            Assert.AreEqual(3, actualParameters[1].Value);
        }

        [Test]
        public void Paging_query_with_constraints_creates_row_number_query_with_constraint_in_aliased_select()
        {
            _querySpecification.Page = new Page(1, 2);
            _querySpecification.QueryExpression = QueryExpression.Create(new EqualsExpression("ColumnName3", "value"));

            var expectedSql = "WITH [CompleteResult] AS (SELECT [ColumnName1], [ColumnName2], ROW_NUMBER() OVER (ORDER BY [IdColumnName]) AS [RowNumber] " +
                              "FROM [TableName] WHERE [ColumnName3] = @ColumnName3Constraint) " +
                              "SELECT [ColumnName1], [ColumnName2] FROM [CompleteResult] WHERE [RowNumber] BETWEEN @LowRowLimit AND @HighRowLimit";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(3, actualParameters.Count);

            Assert.AreEqual("ColumnName3Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual("value", actualParameters[0].Value);

            Assert.AreEqual("HighRowLimit", actualParameters[1].ParameterName);
            Assert.AreEqual(4, actualParameters[1].Value);

            Assert.AreEqual("LowRowLimit", actualParameters[2].ParameterName);
            Assert.AreEqual(3, actualParameters[2].Value);
        }

        [Test]
        public void Paging_query_with_specified_order_by_uses_specified_order_by_in_row_number_over_clause()
        {
            _querySpecification.Page = new Page(1, 2);
            _querySpecification.QueryExpression = QueryExpression.Create(new EqualsExpression("ColumnName3", "value"));
            _querySpecification.OrderByStatements.Add(OrderByStatement.Create("ColumnName3", OrderByDirection.Descending));
            _querySpecification.OrderByStatements.Add(OrderByStatement.Create("ColumnName4", OrderByDirection.Ascending));

            var expectedSql = "WITH [CompleteResult] AS (SELECT [ColumnName1], [ColumnName2], ROW_NUMBER() " +
                              "OVER (ORDER BY [ColumnName3] DESC, [ColumnName4]) AS [RowNumber] " +
                              "FROM [TableName] WHERE [ColumnName3] = @ColumnName3Constraint) " +
                              "SELECT [ColumnName1], [ColumnName2] FROM [CompleteResult] WHERE [RowNumber] BETWEEN @LowRowLimit AND @HighRowLimit";

            var command = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(expectedSql, command.CommandText);
        }
    }
}
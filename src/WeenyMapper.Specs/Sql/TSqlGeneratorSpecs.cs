using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Exceptions;
using WeenyMapper.Extensions;
using WeenyMapper.QueryParsing;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class TSqlGeneratorSpecs
    {
        private TSqlGenerator _generator;
        private SqlQuerySpecification _querySpecification;

        [SetUp]
        public void SetUp()
        {
            var sqlServerCommandFactory = new SqlServerCommandFactory();

            _generator = new TSqlGenerator(sqlServerCommandFactory);

            _querySpecification = new SqlQuerySpecification();

            _querySpecification.ColumnsToSelect = new[] { "ColumnName1", "ColumnName2" };
            _querySpecification.TableName = "TableName";
            _querySpecification.PrimaryKeyColumnName = "IdColumnName";
        }

        [Test]
        public void Generating_select_without_constraints_generates_select_of_escaped_column_names_without_where_clause()
        {
            var query = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("SELECT [ColumnName1], [ColumnName2] FROM [TableName]", query.CommandText);
        }

        [Test]
        public void Generating_select_with_single_constraints_generates_select_with_parameterized_where_clause()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName" };
            _querySpecification.QueryExpression = new EqualsExpression(new PropertyExpression("ColumnName"),
                                                                       new ValueExpression("value"));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("SELECT [ColumnName] FROM [TableName] WHERE [ColumnName] = @ColumnNameConstraint",
                            sqlCommand.CommandText);

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
        public void Generating_join_without_constraints_generates_select_of_properties_from_both_tables_with_join_on_child_foreign_key_and_parent_id()
        {
            var spec2 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Table2Column1", "Table2Column2" },
                    TableName = "TableName2"
                };

            _querySpecification.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "TableName",
                    ChildTableName = "TableName2",
                    ParentPrimaryKeyColumnName = "PrimaryKeyColumnName",
                    ChildForeignKeyColumnName = "ForeignKeyColumnName",
                    SqlQuerySpecification = spec2
                };

            var query = _generator.GenerateSelectQuery(_querySpecification);

            var expectedSql =
                "SELECT [TableName].[ColumnName1] AS \"TableName ColumnName1\", [TableName].[ColumnName2] AS \"TableName ColumnName2\", " +
                "[TableName2].[Table2Column1] AS \"TableName2 Table2Column1\", [TableName2].[Table2Column2] AS \"TableName2 Table2Column2\" " +
                "FROM [TableName] LEFT OUTER JOIN [TableName2] " +
                "ON [TableName].[PrimaryKeyColumnName] = [TableName2].[ForeignKeyColumnName]";

            Assert.AreEqual(expectedSql, query.CommandText);
        }

        [Test]
        public void Generating_join_with_constraints_generates_join_query_with_corresponding_constraints_qualified_with_table_name()
        {
            _querySpecification.QueryExpression = QueryExpression.Create(
                new OrExpression(
                    new EqualsExpression("ColumnName1", 123),
                    new InExpression(new PropertyExpression("ColumnName2"), new ArrayValueExpression(new[] { 1, 2 })),
                    new LikeExpression(new PropertyExpression("ColumnName2"), "likestring")
                        {
                            HasStartingWildCard = true,
                            HasEndingWildCard = true
                        }));

            var spec2 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Table2Column1", "Table2Column2" },
                    TableName = "TableName2"
                };

            _querySpecification.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "TableName",
                    ChildTableName = "TableName2",
                    ParentPrimaryKeyColumnName = "PrimaryKeyColumnName",
                    ChildForeignKeyColumnName = "ForeignKeyColumnName",
                    SqlQuerySpecification = spec2
                };

            var expectedSql =
                "SELECT [TableName].[ColumnName1] AS \"TableName ColumnName1\", [TableName].[ColumnName2] AS \"TableName ColumnName2\", " +
                "[TableName2].[Table2Column1] AS \"TableName2 Table2Column1\", [TableName2].[Table2Column2] AS \"TableName2 Table2Column2\" " +
                "FROM [TableName] LEFT OUTER JOIN [TableName2] " +
                "ON [TableName].[PrimaryKeyColumnName] = [TableName2].[ForeignKeyColumnName] " +
                "WHERE [TableName].[ColumnName1] = @TableName_ColumnName1Constraint " +
                "OR ([TableName].[ColumnName2] IN (@TableName_ColumnName2Constraint, @TableName_ColumnName2Constraint2)) " +
                "OR [TableName].[ColumnName2] LIKE @TableName_ColumnName2Constraint3";

            var query = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = query.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, query.CommandText);
            Assert.AreEqual(4, actualParameters.Count);
            Assert.AreEqual("TableName_ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual(123, actualParameters[0].Value);
            Assert.AreEqual("TableName_ColumnName2Constraint", actualParameters[1].ParameterName);
            Assert.AreEqual(1, actualParameters[1].Value);
            Assert.AreEqual("TableName_ColumnName2Constraint2", actualParameters[2].ParameterName);
            Assert.AreEqual(2, actualParameters[2].Value);
            Assert.AreEqual("TableName_ColumnName2Constraint3", actualParameters[3].ParameterName);
            Assert.AreEqual("%likestring%", actualParameters[3].Value);
        }

        [Test]
        public void Generating_multi_table_join_generates_join_query_with_corresponding_join_clause()
        {
            _querySpecification.QueryExpression = QueryExpression.Create(new EqualsExpression("ColumnName1", 123));

            var spec2 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Table2Column1", "Table2Column2" },
                    TableName = "TableName2"
                };

            var spec3 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Table3Column1" },
                    TableName = "TableName3"
                };

            _querySpecification.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "TableName",
                    ChildTableName = "TableName2",
                    ParentPrimaryKeyColumnName = "PrimaryKeyColumnName",
                    ChildForeignKeyColumnName = "ForeignKeyColumnName",
                    SqlQuerySpecification = spec2
                };

            spec2.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "TableName2",
                    ChildTableName = "TableName3",
                    ParentPrimaryKeyColumnName = "Table2PrimaryKey",
                    ChildForeignKeyColumnName = "Table3ForeignKey",
                    SqlQuerySpecification = spec3
                };

            var expectedSql =
                "SELECT [TableName].[ColumnName1] AS \"TableName ColumnName1\", [TableName].[ColumnName2] AS \"TableName ColumnName2\", " +
                "[TableName2].[Table2Column1] AS \"TableName2 Table2Column1\", [TableName2].[Table2Column2] AS \"TableName2 Table2Column2\", " +
                "[TableName3].[Table3Column1] AS \"TableName3 Table3Column1\" " +
                "FROM [TableName] LEFT OUTER JOIN [TableName2] " +
                "ON [TableName].[PrimaryKeyColumnName] = [TableName2].[ForeignKeyColumnName] " +
                "LEFT OUTER JOIN [TableName3] ON [TableName2].[Table2PrimaryKey] = [TableName3].[Table3ForeignKey] " +
                "WHERE [TableName].[ColumnName1] = @TableName_ColumnName1Constraint";

            var query = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(expectedSql, query.CommandText);
        }

        [Test]
        public void Generating_multi_table_join_from_the_middle_outwards_generates_join_query_with_each_table_only_referenced_once()
        {
            _querySpecification = new SqlQuerySpecification
                {
                    ColumnsToSelect = new[] { "Name" },
                    TableName = "Posts",
                    PrimaryKeyColumnName = "Id"
                };

            var spec2 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Name" },
                    TableName = "Comments"
                };

            var spec3 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Name" },
                    TableName = "Blogs"
                };

            _querySpecification.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "Posts",
                    ChildTableName = "Comments",
                    ParentPrimaryKeyColumnName = "Id",
                    ChildForeignKeyColumnName = "PostId",
                    SqlQuerySpecification = spec2
                };

            spec2.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "Blogs",
                    ChildTableName = "Posts",
                    ParentPrimaryKeyColumnName = "Id",
                    ChildForeignKeyColumnName = "BlogId",
                    SqlQuerySpecification = spec3
                };

            var expectedSql =
                "SELECT [Posts].[Name] AS \"Posts Name\", " +
                "[Comments].[Name] AS \"Comments Name\", " +
                "[Blogs].[Name] AS \"Blogs Name\" " +
                "FROM [Posts] LEFT OUTER JOIN [Comments] " +
                "ON [Posts].[Id] = [Comments].[PostId] " +
                "LEFT OUTER JOIN [Blogs] ON [Blogs].[Id] = [Posts].[BlogId]";

            var query = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(expectedSql, query.CommandText);
        }

        [Test]
        public void Generating_join_with_order_by_generates_select_with_corresponding_order_by_with_qualified_column_name()
        {
            _querySpecification.OrderByStatements.Add(new OrderByStatement("ColumnName3"));
            _querySpecification.OrderByStatements.Add(new OrderByStatement("ColumnName4", OrderByDirection.Descending));

            var spec2 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Table2Column1" },
                    TableName = "TableName2"
                };

            _querySpecification.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "TableName",
                    ChildTableName = "TableName2",
                    ParentPrimaryKeyColumnName = "PrimaryKeyColumnName",
                    ChildForeignKeyColumnName = "ForeignKeyColumnName",
                    SqlQuerySpecification = spec2
                };

            var query = _generator.GenerateSelectQuery(_querySpecification);

            var expectedSql =
                "SELECT [TableName].[ColumnName1] AS \"TableName ColumnName1\", [TableName].[ColumnName2] AS \"TableName ColumnName2\", " +
                "[TableName2].[Table2Column1] AS \"TableName2 Table2Column1\" " +
                "FROM [TableName] LEFT OUTER JOIN [TableName2] " +
                "ON [TableName].[PrimaryKeyColumnName] = [TableName2].[ForeignKeyColumnName] " +
                "ORDER BY [TableName].[ColumnName3], [TableName].[ColumnName4] DESC";

            Assert.AreEqual(expectedSql, query.CommandText);
        }

        [Test]
        public void Generating_ordered_table_join_with_constraint_adds_where_clause_before_order_by_clause()
        {
            _querySpecification.OrderByStatements.Add(new OrderByStatement("ColumnName3"));
            _querySpecification.QueryExpression = QueryExpression.Create(new EqualsExpression("ColumnName1", 123));

            var spec2 = new SqlQuerySpecification
                {
                    ColumnsToSelect = new List<string> { "Table2Column1" },
                    TableName = "TableName2"
                };

            _querySpecification.JoinSpecification = new SqlQueryJoinSpecification
                {
                    ParentTableName = "TableName",
                    ChildTableName = "TableName2",
                    ParentPrimaryKeyColumnName = "PrimaryKeyColumnName",
                    ChildForeignKeyColumnName = "ForeignKeyColumnName",
                    SqlQuerySpecification = spec2
                };

            var expectedSql =
                "SELECT [TableName].[ColumnName1] AS \"TableName ColumnName1\", [TableName].[ColumnName2] AS \"TableName ColumnName2\", " +
                "[TableName2].[Table2Column1] AS \"TableName2 Table2Column1\" " +
                "FROM [TableName] LEFT OUTER JOIN [TableName2] " +
                "ON [TableName].[PrimaryKeyColumnName] = [TableName2].[ForeignKeyColumnName] " +
                "WHERE [TableName].[ColumnName1] = @TableName_ColumnName1Constraint " +
                "ORDER BY [TableName].[ColumnName3]";

            var query = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(expectedSql, query.CommandText);
        }

        [Test]
        public void Insert_command_for_object_has_column_name_and_parameterized_value_for_each_property()
        {
            var propertyValues = new Dictionary<string, object>();

            propertyValues["ColumnName1"] = "value 1";
            propertyValues["ColumnName2"] = "value 2";

            var sqlCommand = _generator.CreateInsertCommand("TableName", propertyValues);

            Assert.AreEqual(
                "INSERT INTO [TableName] ([ColumnName1], [ColumnName2]) VALUES (@ColumnName1, @ColumnName2)",
                sqlCommand.CommandText);

            Assert.AreEqual(2, sqlCommand.Parameters.Count);

            Assert.AreEqual("ColumnName1", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("value 1", sqlCommand.Parameters[0].Value);

            Assert.AreEqual("ColumnName2", sqlCommand.Parameters[1].ParameterName);
            Assert.AreEqual("value 2", sqlCommand.Parameters[1].Value);
        }

        [Test]
        public void Insert_command_for_object_with_identity_id_selects_identity_value()
        {
            var propertyValues = new Dictionary<string, object>();

            propertyValues["ColumnName1"] = "value 1";
            propertyValues["ColumnName2"] = "value 2";

            var scalarCommand = _generator.CreateIdentityInsertCommand("TableName", propertyValues);
            var sqlCommand = scalarCommand.ResultCommand;

            var expectedSql =
                "INSERT INTO [TableName] ([ColumnName1], [ColumnName2]) VALUES (@ColumnName1, @ColumnName2);" +
                "SELECT CAST(@@IDENTITY AS int)";

            Assert.AreEqual(expectedSql, sqlCommand.CommandText);

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

            Assert.AreEqual("SELECT COUNT(*) FROM [TableName] WHERE [ColumnName1] = @ColumnName1Constraint",
                            sqlCommand.CommandText);

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
            _querySpecification.QueryExpression = new EqualsExpression(new PropertyExpression("ColumnName"),
                                                                       new ValueExpression("Value"));
            _querySpecification.TableName = "TableName";

            var expectedSql =
                "SELECT [ColumnName1], [ColumnName2] FROM [TableName] WHERE [ColumnName] = @ColumnNameConstraint";

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

            _querySpecification.QueryExpression = new InExpression(new PropertyExpression("PropertyName"),
                                                                   new ArrayValueExpression(values));

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

            var expectedSql = "SELECT TOP(@RowCountLimitConstraint) [ColumnName1], [ColumnName2] FROM [TableName] " +
                              "WHERE [ColumnName1] = @ColumnName1Constraint";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(2, actualParameters.Count);

            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual(1, actualParameters[0].Value);

            Assert.AreEqual("RowCountLimitConstraint", actualParameters[1].ParameterName);
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

            var expectedSql =
                "WITH [CompleteResult] AS (SELECT [ColumnName1], [ColumnName2], ROW_NUMBER() OVER (ORDER BY [IdColumnName]) AS \"RowNumber\" " +
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
        [ExpectedException(typeof(WeenyMapperException))]
        public void Paging_query_for_entity_without_primary_key_and_without_ordering_throws_exception()
        {
            _querySpecification.PrimaryKeyColumnName = null;
            _querySpecification.Page = new Page(1, 2);

            _generator.GenerateSelectQuery(_querySpecification);
        }

        [Test]
        public void Paging_query_with_constraints_creates_row_number_query_with_constraint_in_aliased_select()
        {
            _querySpecification.Page = new Page(1, 2);
            _querySpecification.QueryExpression = QueryExpression.Create(new EqualsExpression("ColumnName3", "value"));

            var expectedSql =
                "WITH [CompleteResult] AS (SELECT [ColumnName1], [ColumnName2], ROW_NUMBER() OVER (ORDER BY [IdColumnName]) AS \"RowNumber\" " +
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
                              "OVER (ORDER BY [ColumnName3] DESC, [ColumnName4]) AS \"RowNumber\" " +
                              "FROM [TableName] WHERE [ColumnName3] = @ColumnName3Constraint) " +
                              "SELECT [ColumnName1], [ColumnName2] FROM [CompleteResult] WHERE [RowNumber] BETWEEN @LowRowLimit AND @HighRowLimit";

            var command = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(expectedSql, command.CommandText);
        }

        [Test]
        public void Like_expression_with_starting_and_ending_wildcard_is_translated_into_like_query_with_starting_and_ending_wildcard()
        {
            _querySpecification.QueryExpression =
                QueryExpression.Create(new LikeExpression(new PropertyExpression("ColumnName1"), "substring")
                    {
                        HasStartingWildCard = true,
                        HasEndingWildCard = true
                    });

            var expectedSql =
                "SELECT [ColumnName1], [ColumnName2] FROM [TableName] WHERE [ColumnName1] LIKE @ColumnName1Constraint";

            var command = _generator.GenerateSelectQuery(_querySpecification);
            var actualParameters = command.Parameters.SortByParameterName();

            Assert.AreEqual(expectedSql, command.CommandText);

            Assert.AreEqual(1, actualParameters.Count);

            Assert.AreEqual("ColumnName1Constraint", actualParameters[0].ParameterName);
            Assert.AreEqual("%substring%", actualParameters[0].Value);
        }

        [Test]
        public void Like_expression_with_only_starting_wildcard_is_translated_into_like_query_with_only_starting_wildcard()
        {
            _querySpecification.QueryExpression =
                QueryExpression.Create(new LikeExpression(new PropertyExpression("ColumnName1"), "substring")
                    {
                        HasStartingWildCard = true,
                        HasEndingWildCard = false
                    });

            var command = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(1, command.Parameters.Count);
            Assert.AreEqual("%substring", command.Parameters[0].Value);
        }

        [Test]
        public void Like_expression_with_only_ending_wildcard_is_translated_into_like_query_with_only_ending_wildcard()
        {
            _querySpecification.QueryExpression =
                QueryExpression.Create(new LikeExpression(new PropertyExpression("ColumnName1"), "substring")
                    {
                        HasStartingWildCard = false,
                        HasEndingWildCard = true
                    });

            var command = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual(1, command.Parameters.Count);
            Assert.AreEqual("substring%", command.Parameters[0].Value);
        }

        [ExpectedException(typeof(WeenyMapperException))]
        [Test]
        public void Generating_select_with_in_constraint_without_values_throws_exception()
        {
            _querySpecification.QueryExpression = QueryExpression.Create(
                new InExpression(
                    new PropertyExpression("Property"),
                    new ArrayValueExpression(new List<object>())));

            _generator.GenerateSelectQuery(_querySpecification);
        }

        [Test]
        public void Not_expression_results_in_query_with_not_operator_and_inner_expression_parenthesized()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName1" };
            _querySpecification.QueryExpression =
                new RootExpression(
                    new OrExpression(
                        new EqualsExpression("ColumnName3", "value1"),
                        new NotExpression(
                            new AndExpression(
                                new EqualsExpression(new PropertyExpression("ColumnName1"), new ValueExpression("value")),
                                new EqualsExpression(new PropertyExpression("ColumnName2"), new ValueExpression(123))))));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);
            var parameters = sqlCommand.Parameters.SortByParameterName();

            var expectedQuery = "SELECT [ColumnName1] FROM [TableName] " +
                                "WHERE [ColumnName3] = @ColumnName3Constraint OR NOT ([ColumnName1] = @ColumnName1Constraint AND [ColumnName2] = @ColumnName2Constraint)";

            Assert.AreEqual(expectedQuery, sqlCommand.CommandText);

            Assert.AreEqual(3, parameters.Count);
            Assert.AreEqual("ColumnName1Constraint", parameters[0].ParameterName);
            Assert.AreEqual("value", parameters[0].Value);
            Assert.AreEqual("ColumnName2Constraint", parameters[1].ParameterName);
            Assert.AreEqual(123, parameters[1].Value);
            Assert.AreEqual("ColumnName3Constraint", parameters[2].ParameterName);
            Assert.AreEqual("value1", parameters[2].Value);
        }

        [Test]
        public void Single_property_expression_for_boolean_property_generates_explicit_comparison_to_1()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName" };
            _querySpecification.QueryExpression = new PropertyExpression("ColumnName", typeof(bool));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("SELECT [ColumnName] FROM [TableName] WHERE [ColumnName] = 1",
                            sqlCommand.CommandText);

            Assert.AreEqual(0, sqlCommand.Parameters.Count);
        }

        [Test]
        public void Equals_expression_comparing_something_to_null_generates_IS_NULL_query()
        {
            _querySpecification.ColumnsToSelect = new[] { "ColumnName" };
            _querySpecification.QueryExpression = new EqualsExpression(new PropertyExpression("ColumnName", typeof(string)), new ValueExpression(null));

            var sqlCommand = _generator.GenerateSelectQuery(_querySpecification);

            Assert.AreEqual("SELECT [ColumnName] FROM [TableName] WHERE [ColumnName] IS NULL", sqlCommand.CommandText);

            Assert.AreEqual(0, sqlCommand.Parameters.Count);
        }
    }
}
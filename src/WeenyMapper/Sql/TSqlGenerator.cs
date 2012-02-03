using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Exceptions;
using WeenyMapper.Extensions;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class TSqlGenerator : ISqlGenerator
    {
        private readonly IDbCommandFactory _commandFactory;

        public TSqlGenerator(IDbCommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public DbCommand GenerateSelectQuery(SqlQuerySpecification querySpecification)
        {
            if (querySpecification.HasJoinSpecification)
            {
                return GenerateJoinQuery(querySpecification);
            }
            if (querySpecification.IsPagingQuery)
            {
                return GeneratePagingQuery(querySpecification);
            }

            var command = _commandFactory.CreateCommand();

            var selectedColumnString = CreateColumnNameList(querySpecification.ColumnsToSelect, Escape);
            var topString = AppendTopString("", command, querySpecification.RowCountLimit);

            var commandString = string.Format("SELECT{0} {1} FROM {2}", topString, selectedColumnString, Escape(querySpecification.TableName));

            commandString = AppendConstraint(commandString, command, querySpecification.QueryExpression);
            commandString = AppendOrderBy(commandString, querySpecification.OrderByStatements);

            command.CommandText = commandString;

            return command;
        }

        private DbCommand GenerateJoinQuery(SqlQuerySpecification querySpecification)
        {
            var columnSelectStrings = CreateColumnSelectStrings(querySpecification);
            var columnSelectString = string.Join(", ", columnSelectStrings);

            var joinClause = CreateJoinClauses(querySpecification);

            var commandText = string.Format("SELECT {0} FROM {1} {2}",
                columnSelectString,
                Escape(querySpecification.TableName),
                joinClause);

            var command = _commandFactory.CreateCommand();

            commandText = AppendConstraint(commandText, command, querySpecification.QueryExpression, querySpecification.TableName, querySpecification.TableName + "_");
            commandText = AppendOrderBy(commandText, querySpecification.OrderByStatements, Escape(querySpecification.TableName));

            command.CommandText = commandText;

            return command;
        }

        private string CreateJoinClauses(SqlQuerySpecification querySpecification)
        {
            var joinClauses = new List<string>();

            var currentQuerySpecification = querySpecification;

            while (currentQuerySpecification.HasJoinSpecification)
            {
                var joinClause = CreateJoinClause(currentQuerySpecification);
                joinClauses.Add(joinClause);

                currentQuerySpecification = currentQuerySpecification.JoinSpecification.SqlQuerySpecification;
            }

            return string.Join(" ", joinClauses);
        }

        private string CreateJoinClause(SqlQuerySpecification querySpecification)
        {
            return string.Format("LEFT OUTER JOIN {0} ON {1}.{2} = {3}.{4}",
                Escape(querySpecification.JoinSpecification.SqlQuerySpecification.TableName),
                Escape(querySpecification.JoinSpecification.ParentTableName),
                Escape(querySpecification.JoinSpecification.ParentPrimaryKeyColumnName),
                Escape(querySpecification.JoinSpecification.ChildTableName),
                Escape(querySpecification.JoinSpecification.ChildForeignKeyColumnName));
        }

        private IEnumerable<string> CreateColumnSelectStrings(SqlQuerySpecification querySpecification)
        {
            var joinedColumnStrings = Enumerable.Empty<string>();

            if (querySpecification.HasJoinSpecification)
            {
                joinedColumnStrings = CreateColumnSelectStrings(querySpecification.JoinSpecification.SqlQuerySpecification);
            }

            var stringsForCurrentTable = querySpecification.ColumnsToSelect.Select(x => CreateColumnSelectString(x, querySpecification));

            return stringsForCurrentTable.Concat(joinedColumnStrings);
        }

        private string CreateColumnSelectString(string columnName, SqlQuerySpecification querySpecification)
        {
            var alias = string.Format("{0} {1}", querySpecification.TableName, columnName);
            return string.Format("{0}.{1} AS \"{2}\"", Escape(querySpecification.TableName), Escape(columnName), alias);
        }

        private DbCommand GeneratePagingQuery(SqlQuerySpecification querySpecification)
        {
            var command = _commandFactory.CreateCommand();

            if (querySpecification.OrderByStatements.IsEmpty())
            {
                var orderByPrimaryKeyStatement = OrderByStatement.Create(querySpecification.PrimaryKeyColumnName, OrderByDirection.Ascending);
                querySpecification.OrderByStatements.Add(orderByPrimaryKeyStatement);
            }

            var selectedColumnString = CreateColumnNameList(querySpecification.ColumnsToSelect, Escape);

            var constraintString = AppendConstraint("", command, querySpecification.QueryExpression);
            var orderByString = AppendOrderBy("", querySpecification.OrderByStatements).Trim();

            var commandString = string.Format(
                "WITH [CompleteResult] AS (SELECT {0}, ROW_NUMBER() OVER ({1}) AS \"RowNumber\" FROM {2}{3}) " +
                "SELECT {0} FROM [CompleteResult] WHERE [RowNumber] BETWEEN @LowRowLimit AND @HighRowLimit",
                selectedColumnString, orderByString, Escape(querySpecification.TableName), constraintString);

            command.CommandText = commandString;

            command.Parameters.Add(_commandFactory.CreateParameter("LowRowLimit", querySpecification.Page.LowRowLimit));
            command.Parameters.Add(_commandFactory.CreateParameter("HighRowLimit", querySpecification.Page.HighRowLimit));

            command.CommandText = commandString;

            return command;
        }

        private string AppendTopString(string commandText, DbCommand command, int rowCountLimit)
        {
            if (rowCountLimit <= 0)
            {
                return commandText;
            }

            command.Parameters.Add(_commandFactory.CreateParameter("RowCountLimit", rowCountLimit));

            return commandText + string.Format(" TOP(@RowCountLimit)");
        }

        private string AppendOrderBy(string commandString, IEnumerable<OrderByStatement> orderByStatements, string tableName = "")
        {
            if (orderByStatements.IsEmpty())
            {
                return commandString;
            }

            return commandString + " ORDER BY " + string.Join(", ", orderByStatements.Select(x => CreateOrderByString(x, tableName)));
        }

        private static string CreateOrderByString(OrderByStatement orderByStatement, string tableName)
        {
            var direction = orderByStatement.Direction == OrderByDirection.Ascending ? "" : " DESC";
            var columnName = CreateColumnNameString(orderByStatement.PropertyName, tableName);

            return columnName + direction;
        }

        public DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues)
        {
            var columnNamesString = CreateColumnNameList(propertyValues, Escape);
            var parameterNamesString = CreateColumnNameList(propertyValues, x => "@" + x);

            var insertCommand = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", Escape(tableName), columnNamesString, parameterNamesString);

            var command = _commandFactory.CreateCommand();
            command.CommandText = insertCommand;

            AddParameters(command, propertyValues);

            return command;
        }

        public DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, QueryExpression constraintExpression, IDictionary<string, object> columnSetters)
        {
            var updateString = CreateColumnNameList(columnSetters, x => CreateParameterEqualsStatement(x));

            var sql = string.Format("UPDATE {0} SET {1}", Escape(tableName), updateString);
            var command = _commandFactory.CreateCommand();
            command.CommandText = sql;

            var commandText = AppendConstraint(sql, command, constraintExpression);
            AddParameters(command, columnSetters);

            command.CommandText = commandText;

            return command;
        }

        public DbCommand CreateDeleteCommand(string tableName, QueryExpression queryExpression)
        {
            var commandText = string.Format("DELETE FROM {0}", Escape(tableName));

            var command = _commandFactory.CreateCommand();
            command.CommandText = commandText;
            commandText = AppendConstraint(commandText, command, queryExpression);

            command.CommandText = commandText;
            return command;
        }

        public virtual ScalarCommand CreateIdentityInsertCommand(string tableName, IDictionary<string, object> columnValues)
        {
            var scalarCommand = new ScalarCommand();

            var insertCommand = CreateInsertCommand(tableName, columnValues);
            insertCommand.CommandText += ";SELECT CAST(@@IDENTITY AS int)";

            scalarCommand.ResultCommand = insertCommand;

            return scalarCommand;
        }

        public DbCommand CreateCountCommand(string tableName, QueryExpression queryExpression)
        {
            var countQuery = string.Format("SELECT COUNT(*) FROM {0}", Escape(tableName));

            var command = _commandFactory.CreateCommand();

            countQuery = AppendConstraint(countQuery, command, queryExpression);

            command.CommandText = countQuery;

            return command;
        }

        private string CreateColumnNameList(IEnumerable<KeyValuePair<string, object>> propertyValues, Func<string, string> transformation)
        {
            var columnNames = propertyValues.Select(x => x.Key);
            return CreateColumnNameList(columnNames, transformation);
        }

        private string CreateColumnNameList(IEnumerable<string> columnNames, Func<string, string> transformation)
        {
            var escapedColumnNames = columnNames.Select(transformation);
            return string.Join(", ", escapedColumnNames);
        }

        private string AppendConstraint(string commandString, DbCommand command, QueryExpression queryExpression, string columnNamePrefix = "", string parameterNamePrefix = "")
        {
            var newCommandString = commandString;

            var commandParameterFactory = new CommandParameterFactory
                {
                    ParameterNamePrefix = parameterNamePrefix,
                };

            var whereExpression = TSqlExpression.Create(queryExpression, commandParameterFactory, columnNamePrefix);
            var constraintString = whereExpression.ConstraintCommandText;

            if (constraintString != "()" && !string.IsNullOrWhiteSpace(constraintString))
            {
                newCommandString += " WHERE " + whereExpression.ConstraintCommandText;
            }

            command.Parameters.AddRange(whereExpression.CommandParameters.Select(x => _commandFactory.CreateParameter(x.Name, x.Value)).ToArray());

            return newCommandString;
        }

        private void AddParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> columnSetters, string parameterNameSuffix = "")
        {
            foreach (var propertyValue in columnSetters)
            {
                command.Parameters.Add(_commandFactory.CreateParameter(propertyValue.Key + parameterNameSuffix, propertyValue.Value));
            }
        }

        private static string CreateParameterEqualsStatement(string columnName, string parameterNameSuffix = "")
        {
            return string.Format("{0} = @{1}" + parameterNameSuffix, Escape(columnName), columnName);
        }

        private static string Escape(string propertyName)
        {
            if (IsEscaped(propertyName))
            {
                return propertyName;
            }

            return string.Format("[{0}]", propertyName);
        }

        private static bool IsEscaped(string propertyName)
        {
            return propertyName.StartsWith("[") && propertyName.EndsWith("]");
        }

        private static string CreateColumnNameString(string columnName, string tableName)
        {
            var columnNameString = Escape(columnName);

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                columnNameString = Escape(tableName) + "." + columnNameString;
            }

            return columnNameString;
        }

        public class TSqlExpression : IExpressionVisitor
        {
            private readonly ICommandParameterFactory _commandParameterFactory;
            private readonly string _tableName;
            private readonly QueryOptimizer _optimizer = new QueryOptimizer();

            private TSqlExpression(QueryExpression expression, ICommandParameterFactory commandParameterFactory, string tableName)
            {
                _commandParameterFactory = commandParameterFactory;
                _tableName = tableName;
                CommandParameters = new List<CommandParameter>();

                expression.Accept(this);
            }

            public string ConstraintCommandText { get; private set; }
            public IList<CommandParameter> CommandParameters { get; private set; }

            public static TSqlExpression Create(QueryExpression queryExpression, ICommandParameterFactory commandParameterFactory, string columnNamePrefix)
            {
                return new TSqlExpression(queryExpression, commandParameterFactory, columnNamePrefix);
            }

            public void Visit(AndExpression expression)
            {
                VisitPolyadicOperatorExpression(expression, " AND ");
            }

            public void Visit(OrExpression expression)
            {
                VisitPolyadicOperatorExpression(expression, " OR ");
            }

            private void VisitPolyadicOperatorExpression<T>(PolyadicOperatorExpression<T> expression, string operatorString)
                where T : PolyadicOperatorExpression<T>
            {
                var sqlExpressions = expression.Expressions.Select(x => Create(x, _commandParameterFactory, _tableName)).ToList();

                CommandParameters = sqlExpressions.SelectMany(x => x.CommandParameters).ToList();
                var commandText = string.Join(operatorString, sqlExpressions.Select(x => x.ConstraintCommandText));

                ConstraintCommandText = string.Format("({0})", commandText);
            }

            public void Visit(ValueExpression expression) {}

            public void Visit(PropertyExpression expression) {}

            public void Visit(InExpression expression)
            {
                if (expression.ArrayValueExpression.Values.Count() == 0)
                {
                    throw new WeenyMapperException("Can not generate IN constraint from empty collection");
                }

                var columnName = expression.PropertyExpression.PropertyName;

                var newParameters = new List<CommandParameter>();

                foreach (var value in expression.ArrayValueExpression.Values)
                {
                    var commandParameter = _commandParameterFactory.Create(columnName, value);
                    CommandParameters.Add(commandParameter);
                    newParameters.Add(commandParameter);
                }
                var parameterString = string.Join(", ", newParameters.Select(x => x.ReferenceName));
                var columnNameString = CreateColumnNameString(expression.PropertyExpression.PropertyName);

                ConstraintCommandText = string.Format("({0} IN ({1}))", columnNameString, parameterString);
            }

            public void Visit(EqualsExpression expression)
            {
                VisitBinaryComparisonExpression(expression, "=");
            }

            public void Visit(LessOrEqualExpression expression)
            {
                VisitBinaryComparisonExpression(expression, "<=");
            }

            public void Visit(LessExpression expression)
            {
                VisitBinaryComparisonExpression(expression, "<");
            }

            public void Visit(GreaterOrEqualExpression expression)
            {
                VisitBinaryComparisonExpression(expression, ">=");
            }

            public void Visit(GreaterExpression expression)
            {
                VisitBinaryComparisonExpression(expression, ">");
            }

            public void Visit(RootExpression expression)
            {
                if (expression.HasChildExpression)
                {
                    expression.ChildExpression.Accept(this);

                    ConstraintCommandText = _optimizer.ReduceParens(ConstraintCommandText);
                }
            }

            public void Visit(LikeExpression expression)
            {
                var searchString = expression.SearchString;

                if (expression.HasStartingWildCard)
                {
                    searchString = "%" + searchString;
                }
                if (expression.HasEndingWildCard)
                {
                    searchString = searchString + "%";
                }

                var propertyName = expression.PropertyExpression.PropertyName;

                var commandParameter = _commandParameterFactory.Create(propertyName, searchString);
                CommandParameters.Add(commandParameter);

                ConstraintCommandText = string.Format("{0} LIKE {1}", CreateColumnNameString(propertyName), commandParameter.ReferenceName);
            }

            public void Visit(EntityReferenceExpression expression) {}

            private void VisitBinaryComparisonExpression<T>(BinaryComparisonExpression<T> expression, string operatorString)
                where T : BinaryComparisonExpression<T>
            {
                var columnName = expression.PropertyExpression.PropertyName;
                var value = expression.ValueExpression.Value;

                var commandParameter = _commandParameterFactory.Create(columnName, value);

                CommandParameters.Add(commandParameter);

                ConstraintCommandText = commandParameter.ToConstraintString(operatorString, CreateColumnNameString);
            }

            private string CreateColumnNameString(string columnName)
            {
                return TSqlGenerator.CreateColumnNameString(columnName, _tableName);
            }
        }
    }
}
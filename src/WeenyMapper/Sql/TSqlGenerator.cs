using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using WeenyMapper.Extensions;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class TSqlGenerator : ISqlGenerator
    {
        public DbCommand GenerateSelectQuery(SqlQuerySpecification querySpecification)
        {
            if (querySpecification.IsPagingQuery)
            {
                return GeneratePagingQuery(querySpecification);
            }

            var command = new SqlCommand();

            var selectedColumnString = CreateColumnNameList(querySpecification.ColumnsToSelect, Escape);
            var topString = AppendTopString("", command, querySpecification.RowCountLimit);

            var commandString = string.Format("SELECT{0} {1} FROM {2}", topString, selectedColumnString, Escape(querySpecification.TableName));

            commandString = AppendConstraint(commandString, command, querySpecification.QueryExpression);
            commandString = AppendOrderBy(commandString, querySpecification.OrderByStatements);

            command.CommandText = commandString;

            return command;
        }

        private DbCommand GeneratePagingQuery(SqlQuerySpecification querySpecification)
        {
            var command = new SqlCommand();

            if (querySpecification.OrderByStatements.IsEmpty())
            {
                var orderByPrimaryKeyStatement = OrderByStatement.Create(querySpecification.PrimaryKeyColumnName, OrderByDirection.Ascending);
                querySpecification.OrderByStatements.Add(orderByPrimaryKeyStatement);
            }

            var selectedColumnString = CreateColumnNameList(querySpecification.ColumnsToSelect, Escape);

            var constraintString = AppendConstraint("", command, querySpecification.QueryExpression);
            var orderByString = AppendOrderBy("", querySpecification.OrderByStatements).Trim();

            var commandString = string.Format(
                "WITH [CompleteResult] AS (SELECT {0}, ROW_NUMBER() OVER ({1}) AS [RowNumber] FROM {2}{3}) " +
                "SELECT {0} FROM [CompleteResult] WHERE [RowNumber] BETWEEN @LowRowLimit AND @HighRowLimit",
                selectedColumnString, orderByString, Escape(querySpecification.TableName), constraintString);

            command.CommandText = commandString;

            command.Parameters.Add(new SqlParameter("LowRowLimit", querySpecification.Page.LowRowLimit));
            command.Parameters.Add(new SqlParameter("HighRowLimit", querySpecification.Page.HighRowLimit));

            command.CommandText = commandString;

            return command;
        }

        private string AppendTopString(string commandText, DbCommand command, int rowCountLimit)
        {
            if (rowCountLimit <= 0)
            {
                return commandText;
            }

            command.Parameters.Add(new SqlParameter("RowCountLimit", rowCountLimit));

            return commandText + string.Format(" TOP(@RowCountLimit)");
        }

        private string AppendOrderBy(string commandString, IEnumerable<OrderByStatement> orderByStatements)
        {
            if (orderByStatements.IsEmpty())
            {
                return commandString;
            }

            return commandString + " ORDER BY " + string.Join(", ", orderByStatements.Select(CreateOrderByString));
        }

        private static string CreateOrderByString(OrderByStatement orderByStatement)
        {
            var direction = orderByStatement.Direction == OrderByDirection.Ascending ? "" : " DESC";
            var columnName = Escape(orderByStatement.PropertyName);

            return columnName + direction;
        }

        public DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues)
        {
            var columnNamesString = CreateColumnNameList(propertyValues, Escape);
            var parameterNamesString = CreateColumnNameList(propertyValues, x => "@" + x);

            var insertCommand = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", Escape(tableName), columnNamesString, parameterNamesString);

            var sqlCommand = new SqlCommand(insertCommand);

            AddParameters(sqlCommand, propertyValues);

            return sqlCommand;
        }

        public DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, QueryExpression constraintExpression, IDictionary<string, object> columnSetters)
        {
            var updateString = CreateColumnNameList(columnSetters, x => CreateParameterEqualsStatement(x));

            var sql = string.Format("UPDATE {0} SET {1}", Escape(tableName), updateString);
            var command = new SqlCommand(sql);

            var commandText = AppendConstraint(sql, command, constraintExpression);
            AddParameters(command, columnSetters);

            command.CommandText = commandText;

            return command;
        }

        public DbCommand CreateDeleteCommand(string tableName, QueryExpression queryExpression)
        {
            var commandText = string.Format("DELETE FROM {0}", Escape(tableName));

            var command = new SqlCommand(commandText);
            commandText = AppendConstraint(commandText, command, queryExpression);

            command.CommandText = commandText;
            return command;
        }

        public DbCommand CreateCountCommand(string tableName, QueryExpression queryExpression)
        {
            var countQuery = string.Format("SELECT COUNT(*) FROM {0}", Escape(tableName));

            var command = new SqlCommand();

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

        private string AppendConstraint(string commandString, SqlCommand command, QueryExpression queryExpression)
        {
            var newCommandString = commandString;

            var whereExpression = TSqlExpression.Create(queryExpression, new CommandParameterFactory());
            var constraintString = whereExpression.ConstraintCommandText;

            if (constraintString != "()" && !string.IsNullOrWhiteSpace(constraintString))
            {
                newCommandString += " WHERE " + whereExpression.ConstraintCommandText;
            }

            command.Parameters.AddRange(whereExpression.CommandParameters.Select(x => new SqlParameter(x.Name, x.Value)).ToArray());

            return newCommandString;
        }

        private void AddParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> columnSetters, string parameterNameSuffix = "")
        {
            foreach (var propertyValue in columnSetters)
            {
                command.Parameters.Add(new SqlParameter(propertyValue.Key + parameterNameSuffix, propertyValue.Value));
            }
        }

        private static string CreateParameterEqualsStatement(string columnName, string parameterNameSuffix = "")
        {
            return string.Format("{0} = @{1}" + parameterNameSuffix, Escape(columnName), columnName);
        }

        private static string Escape(string propertyName)
        {
            return string.Format("[{0}]", propertyName);
        }

        public class TSqlExpression : IExpressionVisitor
        {
            private readonly ICommandParameterFactory _commandParameterFactory;

            private TSqlExpression(QueryExpression expression, ICommandParameterFactory commandParameterFactory)
            {
                _commandParameterFactory = commandParameterFactory;
                CommandParameters = new List<CommandParameter>();

                expression.Accept(this);
            }

            public string ConstraintCommandText { get; private set; }
            public IList<CommandParameter> CommandParameters { get; private set; }

            public static TSqlExpression Create(QueryExpression queryExpression, ICommandParameterFactory commandParameterFactory)
            {
                return new TSqlExpression(queryExpression, commandParameterFactory);
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
                var sqlExpressions = expression.Expressions.Select(x => Create(x, _commandParameterFactory)).ToList();

                CommandParameters = sqlExpressions.SelectMany(x => x.CommandParameters).ToList();
                var commandText = string.Join(operatorString, sqlExpressions.Select(x => x.ConstraintCommandText));

                ConstraintCommandText = string.Format("({0})", commandText);
            }

            public void Visit(ValueExpression expression) {}

            public void Visit(PropertyExpression expression) {}

            public void Visit(InExpression expression)
            {
                var columnName = expression.PropertyExpression.PropertyName;

                var newParameters = new List<CommandParameter>();

                foreach (var value in expression.ArrayValueExpression.Values)
                {
                    var commandParameter = _commandParameterFactory.Create(columnName, value);
                    CommandParameters.Add(commandParameter);
                    newParameters.Add(commandParameter);
                }
                var parameterString = string.Join(", ", newParameters.Select(x => x.ReferenceName));
                ConstraintCommandText = string.Format("({0} IN ({1}))", Escape(expression.PropertyExpression.PropertyName), parameterString);
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

                    RemoveOutermostCommandTextParens();
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

                ConstraintCommandText = string.Format("[{0}] LIKE {1}", propertyName, commandParameter.ReferenceName);
            }

            private void RemoveOutermostCommandTextParens()
            {
                ConstraintCommandText = ConstraintCommandText
                    .TrimStart('(')
                    .TrimEnd(')');
            }

            private void VisitBinaryComparisonExpression<T>(BinaryComparisonExpression<T> expression, string operatorString)
                where T : BinaryComparisonExpression<T>
            {
                var columnName = expression.PropertyExpression.PropertyName;
                var value = expression.ValueExpression.Value;

                var commandParameter = _commandParameterFactory.Create(columnName, value);

                CommandParameters.Add(commandParameter);

                ConstraintCommandText = commandParameter.ToConstraintString(operatorString, Escape);
            }
        }
    }
}
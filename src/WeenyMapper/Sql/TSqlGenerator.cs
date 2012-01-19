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
        public DbCommand GenerateSelectQuery(QuerySpecification querySpecification)
        {
            var selectedColumnString = CreateColumnNameList(querySpecification.ColumnsToSelect, Escape);
            var commandString = string.Format("select {0} from {1}", selectedColumnString, Escape(querySpecification.TableName));

            var command = new SqlCommand(commandString);

            commandString = AppendConstraint(commandString, command, querySpecification.QueryExpression);
            commandString = AppendOrderBy(commandString, querySpecification.OrderByStatements);

            command.CommandText = commandString;

            return command;
        }

        private string AppendOrderBy(string commandString, IEnumerable<OrderByStatement> orderByStatements)
        {
            if (orderByStatements.IsEmpty())
            {
                return commandString;
            }

            return commandString + " order by " + string.Join(", ", orderByStatements.Select(CreateOrderByString));
        }

        private static string CreateOrderByString(OrderByStatement orderByStatement)
        {
            if (orderByStatement.Direction == OrderByDirection.Ascending)
            {
                return orderByStatement.PropertyName;
            }

            return orderByStatement.PropertyName + " desc";
        }

        public DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues)
        {
            var columnNamesString = CreateColumnNameList(propertyValues, Escape);
            var parameterNamesString = CreateColumnNameList(propertyValues, x => "@" + x);

            var insertCommand = string.Format("insert into {0} ({1}) values ({2})", Escape(tableName), columnNamesString, parameterNamesString);

            var sqlCommand = new SqlCommand(insertCommand);

            AddParameters(sqlCommand, propertyValues);

            return sqlCommand;
        }

        public DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, QueryExpression constraintExpression, IDictionary<string, object> columnSetters)
        {
            var nonPrimaryKeyColumns = columnSetters.Where(x => x.Key != primaryKeyColumn);
            var updateString = CreateColumnNameList(nonPrimaryKeyColumns, x => CreateParameterEqualsStatement(x));

            var sql = string.Format("update {0} set {1}", Escape(tableName), updateString);
            var command = new SqlCommand(sql);

            var commandText = AppendConstraint(sql, command, constraintExpression);
            AddParameters(command, columnSetters);

            command.CommandText = commandText;

            return command;
        }

        public DbCommand CreateDeleteCommand(string tableName, QueryExpression queryExpression)
        {
            var commandText = string.Format("delete from {0}", Escape(tableName));

            var command = new SqlCommand(commandText);
            commandText = AppendConstraint(commandText, command, queryExpression);

            command.CommandText = commandText;
            return command;
        }

        public DbCommand CreateCountCommand(string tableName, QueryExpression queryExpression)
        {
            var countQuery = string.Format("select count(*) from {0}", Escape(tableName));

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
                newCommandString += " where " + whereExpression.ConstraintCommandText;
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
                VisitPolyadicOperatorExpression(expression, " and ");
            }

            public void Visit(OrExpression expression)
            {
                VisitPolyadicOperatorExpression(expression, " or ");
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
                ConstraintCommandText = string.Format("({0} in ({1}))", Escape(expression.PropertyExpression.PropertyName), parameterString);
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
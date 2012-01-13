using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class TSqlGenerator : ISqlGenerator
    {
        public DbCommand GenerateSelectQuery(string tableName, IEnumerable<string> columnNamesToSelect, IDictionary<string, object> constraints)
        {
            var selectedColumnString = CreateColumnNameList(columnNamesToSelect, Escape);
            var commandString = string.Format("select {0} from {1}", selectedColumnString, Escape(tableName));

            return CreateSqlCommandWithWhereClause(commandString, constraints);
        }

        public DbCommand GenerateSelectQuery(string tableName, IEnumerable<string> columnsToSelect, QueryExpression queryExpression)
        {
            var selectedColumnString = CreateColumnNameList(columnsToSelect, Escape);
            var whereClause = TSqlExpression.Create(queryExpression);
            var commandString = string.Format("select {0} from {1} where {2}", selectedColumnString, Escape(tableName), whereClause.ConstraintCommandText);

            var command = new SqlCommand(commandString);
            command.Parameters.AddRange(whereClause.Parameters.ToArray());

            return command;
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

        public DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> propertyValues)
        {
            var setters = new Dictionary<string, object>();
            var constraints = new Dictionary<string, object>();

            foreach (var propertyValue in propertyValues)
            {
                if (propertyValue.Key == primaryKeyColumn)
                {
                    constraints.Add(propertyValue.Key, propertyValue.Value);
                }
                else
                {
                    setters.Add(propertyValue.Key, propertyValue.Value);
                }
            }

            return CreateUpdateCommand(tableName, primaryKeyColumn, constraints, setters);
        }

        public DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> columnConstraints, IDictionary<string, object> columnSetters)
        {
            var nonPrimaryKeyColumns = columnSetters.Where(x => x.Key != primaryKeyColumn);
            var updateString = CreateColumnNameList(nonPrimaryKeyColumns, x => CreateParameterEqualsStatement(x));

            var sql = string.Format("update {0} set {1}", Escape(tableName), updateString);

            var command = CreateSqlCommandWithWhereClause(sql, columnConstraints);

            AddParameters(command, columnSetters);

            return command;
        }

        public DbCommand CreateDeleteCommand(string tableName, IDictionary<string, object> constraints)
        {
            var deleteCommand = string.Format("delete from {0}", Escape(tableName));

            return CreateSqlCommandWithWhereClause(deleteCommand, constraints);
        }

        public DbCommand CreateCountCommand(string tableName, IDictionary<string, object> columnConstraints)
        {
            var countQuery = string.Format("select count(*) from {0}", Escape(tableName));

            return CreateSqlCommandWithWhereClause(countQuery, columnConstraints);
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

        private DbCommand CreateSqlCommandWithWhereClause(string sql, IEnumerable<KeyValuePair<string, object>> columnConstraints)
        {
            sql = AppendWhereClause(sql, columnConstraints);

            var sqlCommand = new SqlCommand(sql);

            AddParameters(sqlCommand, columnConstraints, "Constraint");

            return sqlCommand;
        }

        private string AppendWhereClause(string countQuery, IEnumerable<KeyValuePair<string, object>> columnConstraints)
        {
            if (columnConstraints.Any())
            {
                countQuery += " where " + CreateConstraintClause(columnConstraints);
            }

            return countQuery;
        }

        private void AddParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> columnSetters, string parameterNameSuffix = "")
        {
            foreach (var propertyValue in columnSetters)
            {
                command.Parameters.Add(new SqlParameter(propertyValue.Key + parameterNameSuffix, propertyValue.Value));
            }
        }

        private string CreateConstraintClause(IEnumerable<KeyValuePair<string, object>> columnConstraints)
        {
            var constraintStrings = columnConstraints
                .Select(x => x.Key)
                .Select(columnName => CreateParameterEqualsStatement(columnName, "Constraint"));

            return string.Join(" and ", constraintStrings);
        }

        private static string CreateParameterEqualsStatement(string columnName, string parameterNameSuffix = "")
        {
            return string.Format("{0} = @{1}" + parameterNameSuffix, Escape(columnName), columnName);
        }

        private static string CreateParameterOperatorStatement(string columnName, string operatorString, string parameterNameSuffix = "")
        {
            return string.Format("{0} {1} @{2}" + parameterNameSuffix, Escape(columnName), operatorString, columnName);
        }

        private static string Escape(string propertyName)
        {
            return string.Format("[{0}]", propertyName);
        }

        public class TSqlExpression : IExpressionVisitor
        {
            private TSqlExpression(QueryExpression expression)
            {
                Parameters = new List<SqlParameter>();

                expression.Accept(this);
            }

            public string ConstraintCommandText { get; private set; }
            public IList<SqlParameter> Parameters { get; private set; }

            public static TSqlExpression Create(QueryExpression queryExpression)
            {
                return new TSqlExpression(queryExpression);
            }

            public void Visit(AndExpression expression)
            {
                VisitPolyadicOperatorExpression(expression, " and ");
            }

            public void Visit(OrExpression expression)
            {
                VisitPolyadicOperatorExpression(expression, " or ");
            }

            private void VisitPolyadicOperatorExpression<T>(PolyadicOperatorExpression<T> expression, string separator) where T : PolyadicOperatorExpression<T>
            {
                var sqlExpressions = expression.Expressions.Select(Create);

                Parameters = sqlExpressions.SelectMany(x => x.Parameters).ToList();
                var commandText = string.Join(separator, sqlExpressions.Select(x => x.ConstraintCommandText));

                ConstraintCommandText = string.Format("({0})", commandText);
            }

            public void Visit(ValueExpression expression) {}

            public void Visit(PropertyExpression expression) {}

            public void Visit(InExpression expression)
            {
                throw new NotImplementedException();
            }

            public void Visit(EqualsExpression expression)
            {
                VisitBinaryComparisonExpression(expression);
            }

            public void Visit(LessOrEqualExpression expression)
            {
                VisitBinaryComparisonExpression(expression);
            }

            public void Visit(LessExpression expression)
            {
                VisitBinaryComparisonExpression(expression);
            }

            public void Visit(GreaterOrEqualExpression expression)
            {
                VisitBinaryComparisonExpression(expression);
            }

            public void Visit(GreaterExpression expression)
            {
                VisitBinaryComparisonExpression(expression);
            }

            private void VisitBinaryComparisonExpression<T>(BinaryComparisonExpression<T> expression) where T : BinaryComparisonExpression<T>
            {
                var columnName = expression.PropertyExpression.PropertyName;
                var text = CreateParameterOperatorStatement(columnName, expression.OperatorString, "Constraint");

                Parameters.Add(new SqlParameter(columnName + "Constraint", expression.ValueExpression.Value));

                ConstraintCommandText = text;
            }
        }
    }
}
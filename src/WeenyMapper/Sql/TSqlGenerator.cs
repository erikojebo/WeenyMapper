using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Exceptions;
using WeenyMapper.Extensions;
using WeenyMapper.Mapping;
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

        public DbCommand GenerateSelectQuery(SqlQuery sqlQuery)
        {
            var subQuery = sqlQuery.SubQueries.First();

            if (sqlQuery.IsJoinQuery)
            {
                return GenerateJoinQuery(sqlQuery);
            }
            if (subQuery.IsPagingQuery)
            {
                return GeneratePagingQuery(subQuery);
            }

            var command = _commandFactory.CreateCommand();

            var selectedColumnString = CreateColumnNameList(subQuery);

            command.CommandText = string.Format("SELECT:topClause {0} FROM {1}", selectedColumnString, FromClauseTableIdentifier(subQuery));

            var whereClause = CreateWhereClause(sqlQuery);
            var orderByClause = CreateOrderByClause(subQuery);
            var topClause = new TopClause(subQuery.RowCountLimit, new CommandParameterFactory());

            whereClause.AppendTo(command, _commandFactory);
            orderByClause.AppendTo(command, _commandFactory);
            topClause.InsertWithSpaceAtMarker(command, ":topClause", _commandFactory);

            return command;
        }

        private DbCommand GenerateJoinQuery(SqlQuery sqlQuery)
        {
            var subQuery = sqlQuery.SubQueries.First();

            var columnSelectStrings = CreateColumnSelectStrings(sqlQuery);
            var columnSelectString = string.Join(", ", columnSelectStrings);

            var joinClause = CreateJoinClauses(sqlQuery);

            var commandText = string.Format("SELECT {0} FROM {1} {2}",
                                            columnSelectString,
                                            FromClauseTableIdentifier(subQuery),
                                            joinClause);

            var command = _commandFactory.CreateCommand(commandText);

            var whereClause = CreateWhereClause(sqlQuery);
            var orderByClause = CreateOrderByClause(sqlQuery);

            whereClause.AppendTo(command, _commandFactory);
            orderByClause.AppendTo(command, _commandFactory);

            return command;
        }

        private string CreateJoinClauses(SqlQuery sqlQuery)
        {
            var joinClauses = new List<string>();
            var availableTables = new List<string> { sqlQuery.SubQueries.First().TableName };
            var addedJoins = new HashSet<SqlSubQueryJoin>();

            while (addedJoins.Count < sqlQuery.Joins.Count)
            {
                foreach (var remainingJoin in sqlQuery.Joins.Except(addedJoins).ToList())
                {
                    AliasedSqlSubQuery newSubQuery = null;

                    if (availableTables.Contains(remainingJoin.ChildTableName))
                        newSubQuery = remainingJoin.ParentSubQuery;
                    else if (availableTables.Contains(remainingJoin.ParentTableName))
                        newSubQuery = remainingJoin.ChildSubQuery;

                    if (newSubQuery == null)
                        continue;

                    var joinClause = CreateJoinClause(remainingJoin, newSubQuery);

                    joinClauses.Add(joinClause);
                    addedJoins.Add(remainingJoin);

                    availableTables.Add(remainingJoin.ChildTableName);
                    availableTables.Add(remainingJoin.ParentTableName);
                }
            }

            return string.Join(" ", joinClauses);
        }

        private string CreateJoinClause(SqlSubQueryJoin joinSpec, AliasedSqlSubQuery newSubQuery)
        {
            return string.Format("LEFT OUTER JOIN {0} ON {1}.{2} = {3}.{4}",
                                 FromClauseTableIdentifier(newSubQuery),
                                 Escape(joinSpec.ParentSubQuery.TableIdentifier),
                                 Escape(joinSpec.ParentPrimaryKeyColumnName),
                                 Escape(joinSpec.ChildSubQuery.TableIdentifier),
                                 Escape(joinSpec.ChildForeignKeyColumnName));
        }

        private static string FromClauseTableIdentifier(AliasedSqlSubQuery subQuery)
        {
            if (subQuery.HasCustomAlias)
                return string.Format("{0} AS {1}", Escape(subQuery.TableName), Escape(subQuery.Alias));

            return Escape(subQuery.TableName);
        }

        private DbCommand GeneratePagingQuery(AliasedSqlSubQuery subQuery)
        {
            var command = _commandFactory.CreateCommand();

            if (subQuery.OrderByStatements.IsEmpty() && string.IsNullOrEmpty(subQuery.PrimaryKeyColumnName))
                throw new WeenyMapperException("You have to specify an order by clause for paging queries");

            if (subQuery.OrderByStatements.IsEmpty())
            {
                var orderByPrimaryKeyStatement = OrderByStatement.Create(subQuery.PrimaryKeyColumnName, OrderByDirection.Ascending, 0);
                subQuery.OrderByStatements.Add(orderByPrimaryKeyStatement);
            }

            var selectedColumnString = CreateColumnNameList(subQuery);

            command.CommandText = string.Format("SELECT {0}, ROW_NUMBER() OVER (:orderByClause) AS \"{1}RowNumber\" FROM {2}",
                                                selectedColumnString,
                                                EntityMapper.WeenyMapperGeneratedColumnNamePrefix,
                                                Escape(subQuery.TableName));

            var whereClause = CreateWhereClause(subQuery);
            var orderByClause = CreateOrderByClause(subQuery);

            whereClause.AppendTo(command, _commandFactory);
            orderByClause.InsertAtMarker(command, ":orderByClause", _commandFactory);

            command.CommandText = string.Format("WITH [CompleteResult] AS ({0}) ", command.CommandText);

            var selectString = string.Format(
                "SELECT * FROM [CompleteResult] WHERE [{0}RowNumber] BETWEEN @LowRowLimit AND @HighRowLimit", EntityMapper.WeenyMapperGeneratedColumnNamePrefix);

            command.CommandText += selectString;

            command.Parameters.Add(_commandFactory.CreateParameter("LowRowLimit", subQuery.Page.LowRowLimit));
            command.Parameters.Add(_commandFactory.CreateParameter("HighRowLimit", subQuery.Page.HighRowLimit));

            return command;
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

        public DbCommand CreateUpdateCommand(string tableName, QueryExpression constraintExpression, IDictionary<string, object> columnSetters)
        {
            var updateString = CreateColumnNameList(columnSetters, x => CreateParameterEqualsStatement(x));

            var sql = string.Format("UPDATE {0} SET {1}", Escape(tableName), updateString);
            var command = _commandFactory.CreateCommand();
            command.CommandText = sql;

            var whereClause = CreateWhereClause(constraintExpression);

            whereClause.AppendTo(command, _commandFactory);
            AddParameters(command, columnSetters);

            return command;
        }

        public DbCommand CreateDeleteCommand(string tableName, QueryExpression queryExpression)
        {
            var commandText = string.Format("DELETE FROM {0}", Escape(tableName));

            var command = _commandFactory.CreateCommand(commandText);

            var whereClause = CreateWhereClause(queryExpression);

            whereClause.AppendTo(command, _commandFactory);

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

            var command = _commandFactory.CreateCommand(countQuery);

            var whereClause = CreateWhereClause(queryExpression);

            whereClause.AppendTo(command, _commandFactory);

            return command;
        }

        private IEnumerable<string> CreateColumnSelectStrings(SqlQuery sqlQuery)
        {
            var joinedColumnStrings = new List<string>();
            
            foreach (var subQuery in sqlQuery.SubQueries)
            {
                var stringsForCurrentTable = subQuery.ColumnsToSelect.Select(x => CreateColumnSelectString(x, subQuery));

                joinedColumnStrings.AddRange(stringsForCurrentTable);
            }

            return joinedColumnStrings;
        }

        private string CreateColumnSelectString(string columnName, AliasedSqlSubQuery subQuery)
        {
            var alias = string.Format("{0} {1}", subQuery.TableIdentifier, columnName);
            var columnReference = new ColumnReference(columnName, subQuery.TableIdentifier, Escape);
            return string.Format("{0} AS \"{1}\"", columnReference, alias);
        }

        private string CreateColumnNameList(IEnumerable<KeyValuePair<string, object>> propertyValues, Func<string, string> transformation)
        {
            var columnNames = propertyValues.Select(x => x.Key);
            return CreateColumnNameList(columnNames, transformation);
        }

        private OrderByClause CreateOrderByClause(SqlQuery sqlQuery)
        {
            var combinedOrderByClause = OrderByClause.CreateEmpty();

            foreach (var orderByStatement in sqlQuery.OrderByStatements)
            {
                var parentSubQueryForStatement = sqlQuery.SubQueries.First(x => x.OrderByStatements.Any(y => y == orderByStatement));
                var orderByClause = CreateOrderByClause(orderByStatement, parentSubQueryForStatement);

                combinedOrderByClause = combinedOrderByClause.Combine(orderByClause);
            }

            return combinedOrderByClause;
        }

        private OrderByClause CreateOrderByClause(AliasedSqlSubQuery subQuery)
        {
            return new OrderByClause(subQuery.OrderByStatements, Escape, subQuery.TableIdentifier);
        }

        private OrderByClause CreateOrderByClause(OrderByStatement orderByStatement, AliasedSqlSubQuery subQuery)
        {
            return new OrderByClause(orderByStatement, Escape, subQuery.TableIdentifier);            
        }

        private string CreateColumnNameList(AliasedSqlSubQuery subQuery)
        {
            return CreateColumnNameList(subQuery.ColumnsToSelect, x => new ColumnReference(x, subQuery.TableIdentifier, Escape).ToString());
        }

        private string CreateColumnNameList(IEnumerable<string> columnsNames, Func<string, string> transformation)
        {
            var escapedColumnNames = columnsNames.Select(transformation);
            return string.Join(", ", escapedColumnNames);
        }

        private WhereClause CreateWhereClause(SqlQuery query)
        {
            var combinedWhereClause = WhereClause.CreateEmpty();
            var queryParts = query.GetQueryExpressions();

            var commandParameterFactory = new CommandParameterFactory();

            foreach (var expressionPart in queryParts)
            {
                var parentSubQueryForPart = query.SubQueries.First(x => x.QueryExpressions.Any(q => q == expressionPart));
                
                var whereClause = CreateWhereClause(expressionPart.QueryExpression, parentSubQueryForPart, commandParameterFactory);

                combinedWhereClause = combinedWhereClause.Combine(whereClause, expressionPart.MetaData.CombinationOperation);
            }

            return combinedWhereClause;
        }

        private WhereClause CreateWhereClause(AliasedSqlSubQuery subQuery)
        {
            if (!subQuery.HasQuery)
                return WhereClause.CreateEmpty();

            return CreateWhereClause(subQuery.QueryExpressions.First().QueryExpression, subQuery.TableIdentifier, subQuery.TableIdentifier + "_");
        }

        private WhereClause CreateWhereClause(QueryExpression queryExpression, AliasedSqlSubQuery subQuery, CommandParameterFactory commandParameterFactory)
        {
            commandParameterFactory.ParameterNamePrefix = subQuery.TableIdentifier + "_";
            return CreateWhereClause(queryExpression, subQuery.TableIdentifier, commandParameterFactory);
        }

        private WhereClause CreateWhereClause(QueryExpression queryExpression, string columnNamePrefix = "", string parameterNamePrefix = "")
        {
            var commandParameterFactory = new CommandParameterFactory
            {
                ParameterNamePrefix = parameterNamePrefix,
            };

            return CreateWhereClause(queryExpression, columnNamePrefix, commandParameterFactory);
        }

        private WhereClause CreateWhereClause(QueryExpression queryExpression, string columnNamePrefix, CommandParameterFactory commandParameterFactory)
        {
            var whereExpression = TSqlExpression.Create(queryExpression, commandParameterFactory, columnNamePrefix);
            var whereClause = new WhereClause(whereExpression.ConstraintCommandText);

            whereClause.CommandParameters = whereExpression.CommandParameters;

            return whereClause;
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

            public void Visit(ValueExpression expression)
            {
            }

            public void Visit(PropertyExpression expression)
            {
                if (expression.PropertyType == typeof(bool))
                {
                    var columnName = expression.PropertyName;
                    var columnReference = new ColumnReference(columnName, Escape);
                    ConstraintCommandText = string.Format("{0} = 1", columnReference);
                }
            }

            public void Visit(InExpression expression)
            {
                if (expression.ArrayValueExpression.Values.IsEmpty())
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
                if (expression.ValueExpression.Value == null)
                {
                    var columnName = expression.PropertyExpression.PropertyName;
                    var columnReference = new ColumnReference(columnName, Escape);
                    ConstraintCommandText = string.Format("{0} IS NULL", columnReference);
                }
                else
                {
                    VisitBinaryComparisonExpression(expression, "=");
                }
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

            public void Visit(EntityReferenceExpression expression)
            {
            }

            public void Visit(NotEqualExpression expression)
            {
                VisitBinaryComparisonExpression(expression, "<>");
            }

            public void Visit(NotExpression expression)
            {
                var sqlExpression = Create(expression.Expression, _commandParameterFactory, _tableName);

                CommandParameters = sqlExpression.CommandParameters;
                ConstraintCommandText = string.Format("NOT {0}", sqlExpression.ConstraintCommandText);
            }

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
                return new ColumnReference(columnName, _tableName, Escape).ToString();
            }
        }
    }
}
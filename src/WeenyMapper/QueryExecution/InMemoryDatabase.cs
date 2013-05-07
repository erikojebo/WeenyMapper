using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;
using WeenyMapper.Extensions;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryDatabase
    {
        private static int _lastIdentityId;

        private readonly IConventionReader _conventionReader;
        private readonly IEntityMapper _entityMapper;
        private readonly Dictionary<Type, ResultSet> _tables = new Dictionary<Type, ResultSet>();

        public InMemoryDatabase(IConventionReader conventionReader, IEntityMapper entityMapper)
        {
            _conventionReader = conventionReader;
            _entityMapper = entityMapper;
        }

        public void Add<T>(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        private void Add<T>(T entity)
        {
            var hasIdentityId = _conventionReader.HasIdentityId(typeof(T));

            if (hasIdentityId)
            {
                var idProperty = _conventionReader.TryGetIdProperty(typeof(T));
                idProperty.SetValue(entity, ++_lastIdentityId, null);
            }

            var columnValues = _conventionReader.GetColumnValues(entity);
            var row = new Row(columnValues);

            EnsureTable<T>();

            Table<T>().AddRow(row);
        }

        private void EnsureTable<T>()
        {
            if (!_tables.ContainsKey(typeof(T)))
                _tables[typeof(T)] = new ResultSet();
        }

        public IList<T> Find<T>(ObjectQuery query)
        {
            EnsureTable<T>();

            var matchingRows = Table<T>().Rows.Where(row => MatchesQuery(row, query)).ToList();

            var subQuery = query.GetSubQuery<T>();

            matchingRows = Order(query, matchingRows);
            matchingRows = Limit(subQuery, matchingRows);
            matchingRows = Page(subQuery, matchingRows);

            matchingRows = StripUnselectedColumns(query, matchingRows);

            var resultSet = new ResultSet(matchingRows);

            return _entityMapper.CreateInstanceGraphs<T>(resultSet);
        }

        private List<Row> StripUnselectedColumns(ObjectQuery query, List<Row> matchingRows)
        {
            var columnNamesToSelect = query.SubQueries.First().GetColumnNamesToSelect(_conventionReader);

            var strippedRows = new List<Row>();

            foreach (var row in matchingRows)
            {
                var columnsToSelect = row.ColumnValues.Where(x => columnNamesToSelect.Contains(x.ColumnName));
                var strippedRow = new Row(columnsToSelect);

                strippedRows.Add(strippedRow);
            }

            return strippedRows;
        }

        private List<Row> Order(ObjectQuery query, List<Row> rows)
        {
            var orderedResult = rows.ToList();

            orderedResult.Sort((left, right) =>
                {
                    foreach (var orderByStatement in query.OrderByStatements)
                    {
                        var translatedOrderBy = orderByStatement.Translate(_conventionReader);

                        var leftValue = left.ColumnValues.First(x => x.ColumnName == translatedOrderBy.PropertyName).Value;
                        var rightValue = right.ColumnValues.First(x => x.ColumnName == translatedOrderBy.PropertyName).Value;

                        if (leftValue.GetType().ImplementsInterface<IComparable>())
                        {
                            var leftComparable = (IComparable)leftValue;

                            var result = leftComparable.CompareTo(rightValue);

                            var areDifferent = result != 0;

                            if (areDifferent && orderByStatement.Direction == OrderByDirection.Ascending)
                                return result;
                            if (areDifferent && orderByStatement.Direction == OrderByDirection.Descending)
                                return -1 * result;
                        }
                    }

                    return 0;
                });

            return orderedResult;
        }

        private static List<Row> Limit(AliasedObjectSubQuery subQuery, List<Row> matchingRows)
        {
            if (subQuery.RowCountLimit > 0)
            {
                matchingRows = matchingRows.Take(subQuery.RowCountLimit).ToList();
            }
            return matchingRows;
        }

        private static List<Row> Page(AliasedObjectSubQuery subQuery, List<Row> matchingRows)
        {
            if (subQuery.IsPagingQuery)
            {
                matchingRows = matchingRows.Skip(subQuery.Page.Offset).Take(subQuery.Page.PageSize).ToList();
            }

            return matchingRows;
        }

        private bool MatchesQuery(Row row, ObjectQuery query)
        {
            var queryExpressions = query.SubQueries.SelectMany(x => x.QueryExpressions);
            return queryExpressions.All(q => MatchesQuery(row, q));
        }

        private bool MatchesQuery(Row row, QueryExpressionPart query)
        {
            var translatedExpression = query.QueryExpression.Translate(_conventionReader);

            var matcher = new InMemoryRowMatcher(row, translatedExpression);

            return matcher.IsMatch();
        }

        private ResultSet Table<T>()
        {
            return _tables[typeof(T)];
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryRowMatcher : IExpressionVisitor
    {
        private readonly Row _row;
        private readonly QueryExpression _queryExpression;
        private bool _isMatch;

        public InMemoryRowMatcher(Row row, QueryExpression queryExpression)
        {
            _row = row;
            _queryExpression = queryExpression;
        }

        public void Visit(AndExpression expression)
        {
            foreach (var queryExpression in expression.Expressions)
            {
                var matcher = new InMemoryRowMatcher(_row, queryExpression);

                if (!matcher.IsMatch())
                {
                    _isMatch = false;
                    return;
                }
            }
        }

        public void Visit(OrExpression expression)
        {
            foreach (var queryExpression in expression.Expressions)
            {
                var matcher = new InMemoryRowMatcher(_row, queryExpression);

                if (matcher.IsMatch())
                {
                    return;
                }
            }

            _isMatch = false;
        }

        public void Visit(ValueExpression expression)
        {
        }

        public void Visit(PropertyExpression expression)
        {
            if (expression.PropertyType == typeof(bool))
            {
                var columnName = expression.PropertyName;
                MatchValue(columnName, true);
            }
        }

        public void Visit(InExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(EqualsExpression expression)
        {
            var columnName = expression.PropertyExpression.PropertyName;
            var value = expression.ValueExpression.Value;

            MatchValue(columnName, value);
        }

        private void MatchValue(string columnName, object value)
        {
            var isMatch = IsMatch(columnName, value);

            if (!isMatch)
                _isMatch = false;
        }

        private bool IsMatch(string columnName, object value)
        {
            var columnValue = _row.ColumnValues.First(x => x.ColumnName == columnName).Value;

            return Equals(value, columnValue);
        }

        public void Visit(LessOrEqualExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(LessExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(GreaterOrEqualExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(GreaterExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(RootExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(LikeExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(EntityReferenceExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(NotEqualExpression expression)
        {
            var isColumnValueMatch = IsMatch(expression.PropertyExpression.PropertyName, expression.ValueExpression.Value);

            if (isColumnValueMatch)
                _isMatch = false;
        }

        public void Visit(NotExpression expression)
        {
            var matcher = new InMemoryRowMatcher(_row, expression.Expression);

            if (matcher.IsMatch())
                _isMatch = false;
        }

        public bool IsMatch()
        {
            _isMatch = true;

            _queryExpression.Accept(this);

            return _isMatch;
        }
    }
}
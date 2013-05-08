using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Exceptions;
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

            AddRow(entity);
        }

        private void AddRow(object entity)
        {
            var type = entity.GetType();

            var columnValues = _conventionReader.GetColumnValues(entity);
            var row = new Row(columnValues);

            EnsureTable(type);

            Table(type).AddRow(row);
        }

        public IList<T> Find<T>(ObjectQuery query)
        {
            EnsureTable<T>();


            var subQuery = query.GetSubQuery<T>();

            var matchingRows = Filter<T>(query);

            matchingRows = Order(query, matchingRows);
            matchingRows = Limit(subQuery, matchingRows);
            matchingRows = Page(subQuery, matchingRows);

            matchingRows = StripUnselectedColumns(query, matchingRows);

            var resultSet = new ResultSet(matchingRows);

            return _entityMapper.CreateInstanceGraphs<T>(resultSet);
        }

        private List<Row> Filter<T>(ObjectQuery query)
        {
            return Filter(query, typeof(T));
        }

        private List<Row> Filter(ObjectQuery query, Type type)
        {
            return Table(type).Rows.Where(row => MatchesQuery(row, query)).ToList();
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
            return queryExpressions.All(q => MatchesQuery(row, q.QueryExpression));
        }

        private bool MatchesQuery(Row row, QueryExpression query)
        {
            var translatedExpression = query.Translate(_conventionReader);

            var matcher = new InMemoryRowMatcher(row, translatedExpression);

            return matcher.IsMatch();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query)
        {
            throw new NotImplementedException();
        }

        public void Update(object instance)
        {
            var type = instance.GetType();

            EnsureTable(type);

            var row = GetRowForEntity(instance);

            var table = Table(type);

            table.Remove(row);

            AddRow(instance);
        }

        private Row GetRowForEntity(object instance)
        {
            var type = instance.GetType();
            var table = Table(type);
            
            var primaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName(type);
            var primaryKeyValue = _conventionReader.GetPrimaryKeyValue(instance);
            
            foreach (var row in table.Rows)
            {
                var columnValue = row.GetValue(primaryKeyColumnName);

                if (columnValue != null && columnValue.MatchesValue(primaryKeyValue))
                    return row;
            }

            throw new WeenyMapperException("Could not find any entity of type '{0}' with the primary key '{1}' with value '{2}'", type.FullName, primaryKeyColumnName, primaryKeyValue);
        }

        private void EnsureTable<T>()
        {
            EnsureTable(typeof(T));
        }

        private void EnsureTable(Type type)
        {
            if (!_tables.ContainsKey(type))
                _tables[type] = new ResultSet();
        }

        private ResultSet Table<T>()
        {
            return Table(typeof(T));
        }

        private ResultSet Table(Type type)
        {
            return _tables[type];
        }

        public void Delete(object instance)
        {
            var row = GetRowForEntity(instance);
            Table(instance.GetType()).Remove(row);
        }

        public int Delete<T>(QueryExpression queryExpression)
        {
            var table = Table<T>();

            var rows = table.Rows.Where(x => MatchesQuery(x, queryExpression)).ToList();

            table.Remove(rows);

            return rows.Count;
        }

        public int Update<T>(QueryExpression queryExpression, IDictionary<PropertyInfo, object> setters)
        {
            var table = Table<T>();

            var rows = table.Rows.Where(x => MatchesQuery(x, queryExpression)).ToList();

            var columnSetters = _conventionReader.GetColumnValues<T>(setters);

            foreach (var row in rows)
            {
                Update(row, columnSetters);
            }

            return rows.Count;
        }

        private void Update(Row row, IDictionary<string, object> columnValues)
        {
            var existingColumnValues = row.ColumnValues.Where(x => columnValues.Keys.Contains(x.ColumnName)).ToList();

            foreach (var existingColumnValue in existingColumnValues)
            {
                var newValue = columnValues.First(x => x.Key == existingColumnValue.ColumnName).Value;
                var newColumnValue = existingColumnValue.Copy(newValue);
                row.Remove(existingColumnValue);
                row.Add(newColumnValue);
            }
        }
    }
}
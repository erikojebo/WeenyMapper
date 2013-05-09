using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Exceptions;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution.InMemory
{
    public class InMemoryDatabase
    {
        private static int _lastIdentityId;

        public IConventionReader ConventionReader;
        public IEntityMapper EntityMapper;
        private readonly Dictionary<string, ResultSet> _tables = new Dictionary<string, ResultSet>();

        public InMemoryDatabase(IConventionReader conventionReader, IEntityMapper entityMapper)
        {
            ConventionReader = conventionReader;
            EntityMapper = entityMapper;
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
            var hasIdentityId = ConventionReader.HasIdentityId(typeof(T));

            if (hasIdentityId)
            {
                var idProperty = ConventionReader.TryGetIdProperty(typeof(T));
                idProperty.SetValue(entity, ++_lastIdentityId, null);
            }

            AddRow(entity);
        }

        private void AddRow(object entity)
        {
            var type = entity.GetType();

            var columnValues = ConventionReader.GetColumnValues(entity);
            var row = new Row(columnValues);

            EnsureTable(type);

            Table(type).AddRow(row);
        }

        public IList<T> Find<T>(ObjectQuery query)
        {
            var resultSet = FindResultSet<T>(query);

            var objectRelations = query.Joins.Select(ObjectRelation.Create).ToList();

            if (objectRelations.Any())
            {
                return EntityMapper.CreateInstanceGraphs<T>(resultSet, objectRelations);
            }

            return EntityMapper.CreateInstanceGraphs<T>(resultSet);
        }

        private ResultSet FindResultSet<T>(ObjectQuery query)
        {
            EnsureTable<T>();

            var subQuery = query.GetSubQuery<T>();

            var matchingRows = Filter<T>(query);

            if (query.IsJoinQuery)
            {
                matchingRows = FindWithJoin<T>(query, matchingRows);
            }
            else
            {
                matchingRows = Order(query, matchingRows);
                matchingRows = Limit(subQuery, matchingRows);
                matchingRows = Page(subQuery, matchingRows);
                matchingRows = StripUnselectedColumns(query, matchingRows);
            }            

            return new ResultSet(matchingRows);
        }

        private List<Row> FindWithJoin<T>(ObjectQuery query, List<Row> matchingRows)
        {
            var tableName = ConventionReader.GetTableName<T>();
            matchingRows = matchingRows.Select(x =>
                {
                    var newColumnValues = x.ColumnValues.Select(c => new ColumnValue(tableName + " " + c.ColumnName, c.Value));
                    return new Row(newColumnValues);
                }).ToList();
            return matchingRows;
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
            var columnNamesToSelect = query.SubQueries.First().GetColumnNamesToSelect(ConventionReader);

            var strippedRows = new List<Row>();

            foreach (var row in matchingRows)
            {
                var columnsToSelect = row.ColumnValues.Where(x => columnNamesToSelect.Contains(x.ColumnName));
                var strippedRow = new Row(columnsToSelect);

                strippedRows.Add(strippedRow);
            }

            return strippedRows;
        }

        private List<Row> Order(ObjectQuery query, IEnumerable<Row> rows)
        {
            var orderedResult = rows.ToList();

            var comparer = new InMemoryRowSorter(query, ConventionReader);

            orderedResult.Sort(comparer);

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
            var matcher = new QueryExpressionRowMatcher(row, ConventionReader);

            return matcher.Matches(query.QueryExpressionTree);
        }

        private bool MatchesQuery(Row row, QueryExpression query)
        {
            var translatedExpression = query.Translate(ConventionReader);

            var matcher = new InMemoryRowMatcher(row, translatedExpression);

            return matcher.IsMatch();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query)
        {
            var result = FindResultSet<T>(query);

            return result.Rows.Select(x => x.ColumnValues.First().Value).Cast<TScalar>().ToList();
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
            
            var primaryKeyColumnName = ConventionReader.GetPrimaryKeyColumnName(type);
            var primaryKeyValue = ConventionReader.GetPrimaryKeyValue(instance);
            
            foreach (var row in table.Rows)
            {
                var columnValue = row.GetColumnValue(primaryKeyColumnName);

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
            var tableName = ConventionReader.GetTableName(type);

            if (!_tables.ContainsKey(tableName))
                _tables[tableName] = new ResultSet();
        }

        private ResultSet Table<T>()
        {
            return Table(typeof(T));
        }

        private ResultSet Table(Type type)
        {
            var tableName = ConventionReader.GetTableName(type);

            return _tables[tableName];
        }

        public void Delete(object instance)
        {
            var row = GetRowForEntity(instance);
            Table(instance.GetType()).Remove(row);
        }

        public int Delete<T>(QueryExpression queryExpression)
        {
            var table = Table<T>();
            var rows = FindMatchingRows<T>(queryExpression);

            table.Remove(rows);

            return rows.Count;
        }

        private List<Row> FindMatchingRows<T>(QueryExpression queryExpression)
        {
            var table = Table<T>();

            var rows = table.Rows.Where(x => MatchesQuery(x, queryExpression)).ToList();

            return rows;
        }

        public int Update<T>(QueryExpression queryExpression, IDictionary<PropertyInfo, object> setters)
        {
            var table = Table<T>();

            var rows = table.Rows.Where(x => MatchesQuery(x, queryExpression)).ToList();

            var columnSetters = ConventionReader.GetColumnValues<T>(setters);

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

        public int Count<T>(QueryExpression queryExpression)
        {
            var matches = FindMatchingRows<T>(queryExpression);

            return matches.Count;
        }
    }
}
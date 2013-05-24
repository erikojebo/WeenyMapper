using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Exceptions;
using WeenyMapper.Extensions;
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

        public IList<T> Find<T>(SqlQuery sqlQuery)
        {
            var resultSet = FindResultSet<T>(sqlQuery);

            if (sqlQuery.ObjectRelations.Any())
            {
                return EntityMapper.CreateInstanceGraphs<T>(resultSet, sqlQuery.ObjectRelations, sqlQuery.PrimaryAlias);
            }

            return EntityMapper.CreateInstanceGraphs<T>(resultSet);
        }

        private ResultSet FindResultSet<T>(SqlQuery sqlQuery)
        {
            EnsureTable<T>();

            var matchingRows = Table<T>().Rows;

            if (sqlQuery.IsJoinQuery)
            {
                matchingRows = FindWithJoin(sqlQuery, matchingRows);
                matchingRows = Filter(sqlQuery, matchingRows);
                matchingRows = Order(sqlQuery, matchingRows);
                matchingRows = StripUnselectedColumns(sqlQuery, matchingRows);
            }
            else
            {
                matchingRows = Filter(sqlQuery, matchingRows);
                matchingRows = Order(sqlQuery, matchingRows);
                matchingRows = Limit(sqlQuery, matchingRows);
                matchingRows = Page(sqlQuery, matchingRows);
                matchingRows = StripUnselectedColumns(sqlQuery, matchingRows);
            }

            return new ResultSet(matchingRows);
        }

        private IList<Row> FindWithJoin(SqlQuery sqlQuery, IList<Row> matchingRows)
        {
            var firstTableIdentifier = sqlQuery.SubQueries.First().TableIdentifier;

            var matches = new ResultSet(matchingRows);
            matches = PrefixRows(matches, firstTableIdentifier);

            foreach (var joinPart in sqlQuery.OrderedJoins)
            {
                var table = Table(joinPart.NewSubQuery.TableName);

                table = PrefixRows(table, joinPart.NewSubQuery.TableIdentifier);

                matches = matches.Join(
                    table, 
                    joinPart.Join.ParentSubQuery.TableIdentifier, 
                    joinPart.Join.ParentPrimaryKeyColumnName, 
                    joinPart.Join.ChildSubQuery.TableIdentifier, 
                    joinPart.Join.ChildForeignKeyColumnName);

                AddColumnsNotAddedByJoin(matches, joinPart.Join.ParentSubQuery);
                AddColumnsNotAddedByJoin(matches, joinPart.Join.ChildSubQuery);
            }

            return matches.Rows;
        }

        private void AddColumnsNotAddedByJoin(ResultSet matches, AliasedSqlSubQuery subQuery)
        {
            var allColumns = subQuery.AllSelectableColumnNames
                                             .Select(x => new ColumnValue(subQuery.TableIdentifier, x, null))
                                             .ToList();

            foreach (var row in matches.Rows)
            {
                var missingColumns = allColumns
                    .Where(x => !row.HasColumnValue(x.TableName, x.ColumnName))
                    .Select(x => x.Copy(null))
                    .ToList();

                row.Add(missingColumns);
            }
        }

        private static ResultSet PrefixRows(ResultSet table, string tableName)
        {
            var prefixRows = PrefixRows(table.Rows, tableName);
            return new ResultSet(prefixRows);
        }

        private static IList<Row> PrefixRows(IEnumerable<Row> rows, string tableName)
        {
            return rows.Select(x =>
                {
                    var newColumnValues = x.ColumnValues.Select(c => new ColumnValue(tableName + " " + c.ColumnName, c.Value));
                    return new Row(newColumnValues);
                }).ToList();
        }

        private IList<Row> Filter(SqlQuery sqlQuery, IEnumerable<Row> rows)
        {
            return rows.Where(row => MatchesQuery(row, sqlQuery)).ToList();
        }

        private IList<Row> StripUnselectedColumns(SqlQuery sqlQuery, IEnumerable<Row> matchingRows)
        {
            var columnValuesToSelect = sqlQuery.SubQueries
                                            .SelectMany(x =>
                                                        sqlQuery.GetColumnNamesToSelect(x)
                                                         .Select(y => new ColumnValue(x.TableIdentifier, y, null)))
                                            .ToList();

            var strippedRows = new List<Row>();

            foreach (var row in matchingRows)
            {
                IEnumerable<ColumnValue> columnsToSelect;

                if (row.ColumnValues.IsEmpty())
                    continue;

                if (row.ColumnValues.First().HasTableQualifiedAlias)
                {
                    columnsToSelect = row.ColumnValues.Where(x => columnValuesToSelect.Any(y => y.Alias == x.Alias));
                }
                else
                {
                    columnsToSelect = row.ColumnValues.Where(x => columnValuesToSelect.Any(y => y.ColumnName == x.ColumnName));
                }

                var strippedRow = new Row(columnsToSelect);

                strippedRows.Add(strippedRow);
            }

            return strippedRows;
        }

        private List<Row> Order(SqlQuery sqlQuery, IEnumerable<Row> rows)
        {
            var orderedResult = rows.ToList();

            var comparer = new InMemoryRowSorter(sqlQuery);

            orderedResult.Sort(comparer);

            return orderedResult;
        }

        private static IList<Row> Limit(SqlQuery sqlQuery, IList<Row> matchingRows)
        {
            if (sqlQuery.HasRowCountLimit)
            {
                matchingRows = matchingRows.Take(sqlQuery.RowCountLimit).ToList();
            }
            return matchingRows;
        }

        private static IList<Row> Page(SqlQuery sqlQuery, IList<Row> matchingRows)
        {
            if (sqlQuery.IsPagingQuery)
            {
                matchingRows = matchingRows.Skip(sqlQuery.Page.Offset).Take(sqlQuery.Page.PageSize).ToList();
            }

            return matchingRows;
        }

        private bool MatchesQuery(Row row, SqlQuery query)
        {
            var matcher = new QueryExpressionRowMatcher(row);

            return matcher.Matches(query.QueryExpressionTree);
        }

        private bool MatchesQuery(Row row, QueryExpression query)
        {
            var translatedExpression = query.Translate(ConventionReader);

            var matcher = new InMemoryRowMatcher(row, translatedExpression);

            return matcher.IsMatch();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(SqlQuery sqlQuery)
        {
            var result = FindResultSet<T>(sqlQuery);

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
        
        private ResultSet Table(string tableName)
        {
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
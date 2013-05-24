using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.Exceptions;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Extensions;

namespace WeenyMapper.Sql
{
    public class SqlQuery
    {
        private readonly IConventionReader _conventionReader;
        private readonly IList<OrderByStatement> _orderByStatements;
        private string _primaryAlias;

        public SqlQuery() : this(new ConventionReader(new DefaultConvention()))
        {
        }

        public SqlQuery(IConventionReader conventionReader)
        {
            _conventionReader = conventionReader;

            SubQueries = new List<AliasedSqlSubQuery>();
            Joins = new List<SqlSubQueryJoin>();
            QueryExpressionTree = new EmptyQueryExpressionTree();
            ObjectRelations = new List<ObjectRelation>();
            _orderByStatements = new List<OrderByStatement>();
        }

        public List<AliasedSqlSubQuery> SubQueries { get; set; }
        public List<ObjectRelation> ObjectRelations { get; set; }
        public List<SqlSubQueryJoin> Joins { get; set; }
        public QueryExpressionTree QueryExpressionTree { get; set; }
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }

        public string PrimaryAlias
        {
            get
            {
                if (_primaryAlias == null && !SubQueries[0].Alias.IsNullOrWhiteSpace())
                    return SubQueries[0].Alias;

                return _primaryAlias;
            }
            set { _primaryAlias = value; }
        }

        public bool IsUnorderedPagingQuery()
        {
            return _orderByStatements.IsEmpty() && IsPagingQuery;
        }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public bool IsJoinQuery
        {
            get { return Joins.Any(); }
        }

        public bool HasRowCountLimit
        {
            get { return RowCountLimit > 0; }
        }

        public string StartingTableAlias
        {
            set { SubQueries[0].Alias = value; }
        }
        
        public IList<OrderByStatement> OrderByStatements
        {
            get
            {
                if (IsUnorderedPagingQuery())
                {
                    var subQuery = SubQueries.First();

                    var primaryKeyColumnName = subQuery.PrimaryKeyColumnName;

                    if (primaryKeyColumnName.IsNullOrWhiteSpace())
                        throw new WeenyMapperException("You have to specify an order by clause for paging queries");

                    return OrderByStatement.Create(primaryKeyColumnName, OrderByDirection.Ascending, subQuery.TableIdentifier).AsList();

                }
                return _orderByStatements.ToList();
            }
        }

        public void AddJoin(SqlSubQueryJoin joinSpec, string childAlias, string parentAlias)
        {
            joinSpec.ParentSubQuery = GetSubQuery(joinSpec.ParentTableName, parentAlias);
            joinSpec.ChildSubQuery = GetSubQuery(joinSpec.ChildTableName, childAlias);

            Joins.Add(joinSpec);
        }

        private AliasedSqlSubQuery GetSubQuery(string tableName, string alias)
        {
            return SubQueries.FirstOrDefault(x => x.TableName == tableName && x.Alias == alias);
        }

        public void AddConjunctionExpression(string tableIdentifier, QueryExpression queryExpression)
        {
            var leaf = new TranslatedQueryExpressionTreeLeaf(queryExpression, tableIdentifier);
            QueryExpressionTree = QueryExpressionTree.And(leaf);
        }
        
        public void AddConjunctionExpression(Type type, string alias, QueryExpression queryExpression)
        {
            var subQuery = GetOrCreateSubQuery(alias, type);
            AddConjunctionExpression(subQuery.TableIdentifier, queryExpression.Translate(_conventionReader));
        }

        public void AddDisjunctionExpression(Type type, string alias, QueryExpression queryExpression)
        {
            var subQuery = GetOrCreateSubQuery(alias, type);
            AddDisjunctionExpression(subQuery.TableIdentifier, queryExpression.Translate(_conventionReader));
        }

        public void AddDisjunctionExpression(string tableIdentifier, QueryExpression queryExpression)
        {
            var leaf = new TranslatedQueryExpressionTreeLeaf(queryExpression, tableIdentifier);
            QueryExpressionTree = QueryExpressionTree.Or(leaf);
        }

        public static SqlQuery Create<T>(IConventionReader conventionReader)
        {
            var sqlQuery = new SqlQuery(conventionReader);
         
            sqlQuery.EnsureSubQuery<T>();
            
            return sqlQuery;
        }

        public void EnsureSubQuery<T>(string alias=null)
        {
            EnsureSubQuery(alias, typeof(T));
        }

        public void EnsureSubQuery(string alias, Type type)
        {
            var tableName = _conventionReader.GetTableName(type);

            if (!HasSubQuery(tableName, alias))
            {
                CreateSubQuery(alias, type);
            }
        }

        private bool HasSubQuery(string tableName, string alias = null)
        {
            return SubQueries.Any(x => x.TableName == tableName && x.Alias == alias);
        }
        
        private bool HasSubQueryForIdentifier(string tableIdentifier)
        {
            return SubQueries.Any(x => x.TableIdentifier == tableIdentifier);
        }

        private AliasedSqlSubQuery GetOrCreateSubQuery<T>(string alias)
        {
            return GetOrCreateSubQuery(alias, typeof(T));
        }

        public AliasedSqlSubQuery GetOrCreateSubQuery(string alias, Type type)
        {
            EnsureSubQuery(alias, type);
            return GetSubQuery(_conventionReader.GetTableName(type), alias);
        }

        private void CreateSubQuery(string alias, Type type)
        {
            var subQuery = AliasedSqlSubQuery.Create(alias, type, _conventionReader);

            SubQueries.Add(subQuery);
        }

        public void AddPropertiesToSelect<T>(string alias, IEnumerable<string> propertyNames)
        {
            var subQuery = GetOrCreateSubQuery<T>(alias);
            
            var columnsToSelect = propertyNames.Select(x => _conventionReader.GetColumnName<T>(x)).ToList();

            subQuery.ExplicitlySpecifiedColumnsToSelect.AddRange(columnsToSelect);
        }

        public IEnumerable<string> GetColumnNamesToSelect(string alias, Type type)
        {
            var subQuery = GetOrCreateSubQuery(alias, type);
            return GetColumnNamesToSelect(subQuery);
        }

        public IEnumerable<string> GetColumnNamesToSelect(AliasedSqlSubQuery subQuery)
        {
            var hasExplicitColumnsToSelect = SubQueries.Any(x => x.HasExplicitlySpecifiedColumnsToSelect);

            if (hasExplicitColumnsToSelect)
                return subQuery.ExplicitlySpecifiedColumnsToSelect;

            return subQuery.AllSelectableColumnNames;
        }

        public void AddOrderByStatements<T>(IEnumerable<string> propertyNames, OrderByDirection orderByDirection, string alias)
        {
            var subQuery = GetOrCreateSubQuery<T>(alias);

            var orderByStatements = propertyNames
                .Select(x => _conventionReader.GetColumnName<T>(x))
                .Select(x => OrderByStatement.Create(x, orderByDirection, subQuery.TableIdentifier));

            _orderByStatements.AddRange(orderByStatements);
        }

        public void AddOrderByStatement(OrderByStatement orderByStatement)
        {
            _orderByStatements.Add(orderByStatement);
        }

        public void AddJoin(ObjectSubQueryJoin joinSpecification, string childAlias, string parentAlias)
        {
            EnsureSubQuery(childAlias, joinSpecification.ChildType);
            EnsureSubQuery(parentAlias, joinSpecification.ParentType);

            string manyToOneForeignKeyColumnName;

            if (joinSpecification.HasChildProperty)
                manyToOneForeignKeyColumnName = _conventionReader.GetManyToOneForeignKeyColumnName(joinSpecification.ChildProperty);
            else
                manyToOneForeignKeyColumnName = _conventionReader.GetColumnName(joinSpecification.ChildToParentForeignKeyProperty);

            var joinSpec = new SqlSubQueryJoin
            {
                ChildTableName = _conventionReader.GetTableName(joinSpecification.ChildType),
                ParentTableName = _conventionReader.GetTableName(joinSpecification.ParentType),
                ChildForeignKeyColumnName = manyToOneForeignKeyColumnName,
                ParentPrimaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName(joinSpecification.ParentType),
            };

            AddJoin(joinSpec, childAlias, parentAlias);

            ObjectRelations.Add(ObjectRelation.Create(joinSpecification, childAlias, parentAlias));
        }

        public IList<SqlSubQueryJoinPart> OrderedJoins
        {
            get
            {
                var joins = new List<SqlSubQueryJoinPart>();

                var availableTables = new List<string> { SubQueries.First().TableName };
                var addedJoins = new HashSet<SqlSubQueryJoin>();

                int iterationCounter = 0;

                while (addedJoins.Count < Joins.Count)
                {
                    foreach (var remainingJoin in Joins.Except(addedJoins).ToList())
                    {
                        AliasedSqlSubQuery newSubQuery = null;

                        if (availableTables.Contains(remainingJoin.ChildTableName))
                            newSubQuery = remainingJoin.ParentSubQuery;
                        else if (availableTables.Contains(remainingJoin.ParentTableName))
                            newSubQuery = remainingJoin.ChildSubQuery;

                        if (newSubQuery == null)
                            continue;

                        addedJoins.Add(remainingJoin);
                        joins.Add(new SqlSubQueryJoinPart
                            {
                                Join = remainingJoin,
                                NewSubQuery = newSubQuery
                            });

                        availableTables.Add(remainingJoin.ChildTableName);
                        availableTables.Add(remainingJoin.ParentTableName);
                    }

                    iterationCounter += 1;

                    if (iterationCounter > SubQueries.Count + 1)
                        throw new WeenyMapperException("The join query seems to be invalid. Make sure that you have added joins for each table that needs to be included.");
                }

                return joins;
            }
        }
    }
}
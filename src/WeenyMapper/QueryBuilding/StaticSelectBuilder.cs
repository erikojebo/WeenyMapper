using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WeenyMapper.Async;
using WeenyMapper.Extensions;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryBuilding
{
    public class StaticSelectBuilder<T> : StaticCommandBuilderBase<T> where T : new()
    {
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private readonly IExpressionParser _expressionParser;
        private readonly ObjectQuery _query = new ObjectQuery();

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor, IExpressionParser expressionParser)
        {
            _objectQueryExecutor = objectQueryExecutor;
            _expressionParser = expressionParser;

            var subQuery = new AliasedObjectSubQuery(typeof(T));

            _query.SubQueries.Add(subQuery);
        }

        public StaticSelectBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            return AndWhere(queryExpression);
        }

        public StaticSelectBuilder<T> Where<TAliasedEntity>(string alias, Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            return AndWhere(alias, queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere(Expression<Func<T, bool>> queryExpression)
        {
            return AndWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere<TAliasedEntity>(Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            return AndWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere<TAliasedEntity>(string alias, Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            _query.AddConjunctionExpression<TAliasedEntity>(alias, _expressionParser.Parse(queryExpression));
            
            return this;
        }

        public StaticSelectBuilder<T> OrWhere(Expression<Func<T, bool>> queryExpression)
        {
            return OrWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> OrWhere<TAliasedEntity>(Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            return OrWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> OrWhere<TAliasedEntity>(string alias, Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            _query.AddDisjunctionExpression<TAliasedEntity>(alias, _expressionParser.Parse(queryExpression));
            return this;
        }

        public T Execute()
        {
            return ExecuteList().FirstOrDefault();
        }

        public IList<T> ExecuteList()
        {
            return _objectQueryExecutor.Find<T>(_query);
        }

        public StaticSelectBuilder<T> Select(params Expression<Func<T, object>>[] propertySelectors)
        {
            return Select<T>(propertySelectors);
        }

        public StaticSelectBuilder<T> Select<TAliasedEntity>(params Expression<Func<TAliasedEntity, object>>[] propertySelectors)
        {
            return Select(null, propertySelectors);
        }

        public StaticSelectBuilder<T> Select<TAliasedEntity>(string alias, params Expression<Func<TAliasedEntity, object>>[] propertySelectors)
        {
            foreach (var propertySelector in propertySelectors)
            {
                var subQuery = _query.GetOrCreateSubQuery<TAliasedEntity>(alias);

                string propertyName = GetPropertyName(propertySelector);

                subQuery.PropertiesToSelect.Add(propertyName);    
            }
            
            return this;
        }

        public void ExecuteAsync(Action<T> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(Execute, callback, errorCallback);
        }

        public void ExecuteListAsync(Action<IList<T>> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(ExecuteList, callback, errorCallback);
        }

        public void ExecuteScalarAsync<TScalar>(Action<TScalar> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(ExecuteScalar<TScalar>, callback, errorCallback);
        }

        public TScalar ExecuteScalar<TScalar>()
        {
            return _objectQueryExecutor.FindScalar<T, TScalar>(_query);
        }

        public void ExecuteScalarListAsync<TScalar>(Action<IList<TScalar>> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(ExecuteScalarList<TScalar>, callback, errorCallback);
        }

        public IList<TScalar> ExecuteScalarList<TScalar>()
        {
            return _objectQueryExecutor.FindScalarList<T, TScalar>(_query);
        }

        public StaticSelectBuilder<T> OrderBy(params Expression<Func<T, object>>[] getters)
        {
            return OrderBy<T>(getters);            
        }

        public StaticSelectBuilder<T> OrderBy<TEntity>(params Expression<Func<TEntity, object>>[] getters)
        {
            return OrderBy(null, getters);
        }

        public StaticSelectBuilder<T> OrderBy<TEntity>(string alias, params Expression<Func<TEntity, object>>[] getters)
        {
            AddOrderByStatements(getters, OrderByDirection.Ascending, alias);
            return this;
        }

        public StaticSelectBuilder<T> OrderByDescending(params Expression<Func<T, object>>[] getters)
        {
            return OrderByDescending<T>(getters);
        }

        public StaticSelectBuilder<T> OrderByDescending<TEntity>(params Expression<Func<TEntity, object>>[] getters)
        {
            return OrderByDescending(null, getters);
        }

        public StaticSelectBuilder<T> OrderByDescending<TEntity>(string alias, params Expression<Func<TEntity, object>>[] getters)
        {
            AddOrderByStatements(getters, OrderByDirection.Descending, alias);
            return this;
        }

        private void AddOrderByStatements<TEntity>(IEnumerable<Expression<Func<TEntity, object>>> getters, OrderByDirection orderByDirection, string alias = null)
        {
            var subQuery = _query.GetOrCreateSubQuery<TEntity>(alias);

            var nextOrderByOrderingIndex = GetNextOrderByOrderIndex();

            var orderByStatements = getters
                .Select(GetPropertyName)
                .Select(x => OrderByStatement.Create<TEntity>(x, orderByDirection, nextOrderByOrderingIndex++));

            subQuery.OrderByStatements.AddRange(orderByStatements);
        }

        private int GetNextOrderByOrderIndex()
        {
            var existingOrderByStatements = _query.SubQueries.SelectMany(x => x.OrderByStatements).OrderByDescending(x => x.OrderIndex);

            if (existingOrderByStatements.Any())
                return existingOrderByStatements.First().OrderIndex + 1;

            return 0;
        }

        public StaticSelectBuilder<T> Top(int rowCount)
        {
            var subQuery = _query.GetSubQuery<T>();

            subQuery.RowCountLimit = rowCount;
            return this;
        }

        public StaticSelectBuilder<T> Page(int pageIndex, int pageSize)
        {
            var subQuery = _query.GetSubQuery<T>();

            subQuery.Page = new Page { PageIndex = pageIndex, PageSize = pageSize };
            return this;
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, object>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var parentPropertyInfo = Reflector<T>.GetProperty(parentProperty);
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);

            var joinSpecification = ObjectSubQueryJoin.CreateParentToChild(parentPropertyInfo, foreignKeyPropertyInfo);

            Join<T, TChild>(joinSpecification, childAlias, parentAlias);

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent>(Expression<Func<T, TParent>> childProperty, string childAlias = null, string parentAlias = null)
        {
            var childPropertyInfo = Reflector<T>.GetProperty(childProperty);

            var joinSpecification = ObjectSubQueryJoin.CreateChildToParent(childPropertyInfo, typeof(TParent));

            Join<TParent, T>(joinSpecification, childAlias, parentAlias);

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(
            Expression<Func<TParent, IList<TChild>>> parentProperty,
            Expression<Func<TChild, TParent>> childProperty, string childAlias = null, string parentAlias = null)
        {
            var parentPropertyInfo = Reflector<TParent>.GetProperty(parentProperty);
            var childPropertyInfo = Reflector<TChild>.GetProperty(childProperty);

            var joinSpecification = ObjectSubQueryJoin.CreateTwoWay(parentPropertyInfo, childPropertyInfo);

            Join<TParent, TChild>(joinSpecification, childAlias, parentAlias);

            return this;
        }

        private void Join<TParent, TChild>(ObjectSubQueryJoin joinSpecification, string childAlias, string parentAlias)
        {
            _query.AddJoin<TParent, TChild>(joinSpecification, childAlias, parentAlias);
        }
    }
}
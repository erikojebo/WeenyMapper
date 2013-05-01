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
        private readonly AliasedObjectSubQuery _subQuery;
        private AliasedObjectSubQuery _latestSubQuery;

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor, IExpressionParser expressionParser)
        {
            _objectQueryExecutor = objectQueryExecutor;
            _expressionParser = expressionParser;

            _subQuery = new AliasedObjectSubQuery(typeof(T));
            _latestSubQuery = _subQuery;

            _query.SubQueries.Add(_subQuery);
        }

        public StaticSelectBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            return AndWhere(queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere(Expression<Func<T, bool>> queryExpression)
        {
            if (Equals(_subQuery.QueryExpression, QueryExpression.Create()))
                _subQuery.QueryExpression = _expressionParser.Parse(queryExpression);
            else
                _subQuery.QueryExpression = new AndExpression(_subQuery.QueryExpression, _expressionParser.Parse(queryExpression));

            return this;
        }

        public StaticSelectBuilder<T> OrWhere(Expression<Func<T, bool>> queryExpression)
        {
            if (Equals(_subQuery.QueryExpression, QueryExpression.Create()))
                _subQuery.QueryExpression = _expressionParser.Parse(queryExpression);
            else
                _subQuery.QueryExpression = new OrExpression(_subQuery.QueryExpression, _expressionParser.Parse(queryExpression));

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

        public StaticSelectBuilder<T> Select<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);

            _subQuery.PropertiesToSelect.Add(propertyName);

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
            AddOrderByStatements(getters, OrderByDirection.Ascending);
            return this;
        }

        public StaticSelectBuilder<T> OrderByDescending(params Expression<Func<T, object>>[] getters)
        {
            AddOrderByStatements(getters, OrderByDirection.Descending);
            return this;
        }

        private void AddOrderByStatements(IEnumerable<Expression<Func<T, object>>> getters, OrderByDirection orderByDirection)
        {
            var orderByStatements = getters
                .Select(GetPropertyName)
                .Select(x => OrderByStatement.Create(x, orderByDirection));

            _subQuery.OrderByStatements.AddRange(orderByStatements);
        }

        public StaticSelectBuilder<T> Top(int rowCount)
        {
            _subQuery.RowCountLimit = rowCount;
            return this;
        }

        public StaticSelectBuilder<T> Page(int pageIndex, int pageSize)
        {
            _subQuery.Page = new Page { PageIndex = pageIndex, PageSize = pageSize };
            return this;
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, object>> foreignKeyProperty)
        {
            var parentPropertyInfo = Reflector<T>.GetProperty(parentProperty);
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);

            _latestSubQuery.JoinSpecification = ObjectSubQueryJoin.CreateParentToChild(parentPropertyInfo, foreignKeyPropertyInfo);

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent>(Expression<Func<T, TParent>> childProperty)
        {
            var childPropertyInfo = Reflector<T>.GetProperty(childProperty);

            _latestSubQuery.JoinSpecification = ObjectSubQueryJoin.CreateChildToParent(childPropertyInfo, typeof(TParent));

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(
            Expression<Func<TParent, IList<TChild>>> parentProperty,
            Expression<Func<TChild, TParent>> childProperty)
        {
            var nextType = typeof(TChild);

            if (TypesInQuery.Contains(typeof(TChild)))
            {
                nextType = typeof(TParent);
            }

            var parentPropertyInfo = Reflector<TParent>.GetProperty(parentProperty);
            var childPropertyInfo = Reflector<TChild>.GetProperty(childProperty);

            _latestSubQuery.JoinSpecification = ObjectSubQueryJoin.CreateTwoWay(parentPropertyInfo, childPropertyInfo);
            _latestSubQuery.JoinSpecification.AliasedObjectSubQuery = new AliasedObjectSubQuery(nextType);

            _latestSubQuery = _latestSubQuery.JoinSpecification.AliasedObjectSubQuery;

            return this;
        }

        private IEnumerable<Type> TypesInQuery
        {
            get
            {
                var querySpecification = _subQuery;

                yield return querySpecification.ResultType;

                while (querySpecification.HasJoinSpecification)
                {
                    querySpecification = querySpecification.JoinSpecification.AliasedObjectSubQuery;

                    yield return querySpecification.ResultType;
                } 
            }
        }
    }
}
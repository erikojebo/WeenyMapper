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
        private readonly ObjectQuerySpecification _querySpecification;
        private ObjectQuerySpecification _latestQuerySpecification;

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor, IExpressionParser expressionParser)
        {
            _objectQueryExecutor = objectQueryExecutor;
            _expressionParser = expressionParser;

            _querySpecification = new ObjectQuerySpecification(typeof(T));
            _latestQuerySpecification = _querySpecification;
        }

        public StaticSelectBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            return AndWhere(queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere(Expression<Func<T, bool>> queryExpression)
        {
            if (Equals(_querySpecification.QueryExpression, QueryExpression.Create()))
                _querySpecification.QueryExpression = _expressionParser.Parse(queryExpression);
            else
                _querySpecification.QueryExpression = new AndExpression(_querySpecification.QueryExpression, _expressionParser.Parse(queryExpression));

            return this;
        }

        public StaticSelectBuilder<T> OrWhere(Expression<Func<T, bool>> queryExpression)
        {
            if (Equals(_querySpecification.QueryExpression, QueryExpression.Create()))
                _querySpecification.QueryExpression = _expressionParser.Parse(queryExpression);
            else
                _querySpecification.QueryExpression = new OrExpression(_querySpecification.QueryExpression, _expressionParser.Parse(queryExpression));

            return this;
        }

        public T Execute()
        {
            return ExecuteList().FirstOrDefault();
        }

        public IList<T> ExecuteList()
        {
            return _objectQueryExecutor.Find<T>(_querySpecification);
        }

        public StaticSelectBuilder<T> Select<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);

            _querySpecification.PropertiesToSelect.Add(propertyName);

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
            TaskRunner.Run<TScalar>(ExecuteScalar<TScalar>, callback, errorCallback);
        }

        public TScalar ExecuteScalar<TScalar>()
        {
            return _objectQueryExecutor.FindScalar<T, TScalar>(_querySpecification);
        }

        public void ExecuteScalarListAsync<TScalar>(Action<IList<TScalar>> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run<IList<TScalar>>(ExecuteScalarList<TScalar>, callback, errorCallback);
        }

        public IList<TScalar> ExecuteScalarList<TScalar>()
        {
            return _objectQueryExecutor.FindScalarList<T, TScalar>(_querySpecification);
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

            _querySpecification.OrderByStatements.AddRange(orderByStatements);
        }

        public StaticSelectBuilder<T> Top(int rowCount)
        {
            _querySpecification.RowCountLimit = rowCount;
            return this;
        }

        public StaticSelectBuilder<T> Page(int pageIndex, int pageSize)
        {
            _querySpecification.Page = new Page { PageIndex = pageIndex, PageSize = pageSize };
            return this;
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty)
        {
            var parentPropertyInfo = Reflector<T>.GetProperty(parentProperty);

            _latestQuerySpecification.JoinSpecification = ObjectQueryJoinSpecification.CreateParentToChild(parentPropertyInfo, typeof(TChild));

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent>(Expression<Func<T, TParent>> childProperty)
        {
            var childPropertyInfo = Reflector<T>.GetProperty(childProperty);

            _latestQuerySpecification.JoinSpecification = ObjectQueryJoinSpecification.CreateChildToParent(childPropertyInfo, typeof(TParent));

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(
            Expression<Func<TParent, IList<TChild>>> parentProperty,
            Expression<Func<TChild, TParent>> childProperty)
        {
            var nextType = typeof(TChild);

            if (nextType == _latestQuerySpecification.ResultType)
            {
                nextType = typeof(TParent);
            }

            var parentPropertyInfo = Reflector<TParent>.GetProperty(parentProperty);
            var childPropertyInfo = Reflector<TChild>.GetProperty(childProperty);

            _latestQuerySpecification.JoinSpecification = ObjectQueryJoinSpecification.CreateTwoWay(parentPropertyInfo, childPropertyInfo);
            _latestQuerySpecification.JoinSpecification.ObjectQuerySpecification = new ObjectQuerySpecification(nextType);

            _latestQuerySpecification = _latestQuerySpecification.JoinSpecification.ObjectQuerySpecification;

            return this;
        }
    }
}
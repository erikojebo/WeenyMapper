using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WeenyMapper.Async;
using WeenyMapper.Exceptions;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.Sql;
using WeenyMapper.Extensions;

namespace WeenyMapper.QueryBuilding
{
    public class StaticSelectBuilder<T> : StaticCommandBuilderBase<T> where T : new()
    {
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private readonly IExpressionParser _expressionParser;
        private readonly ObjectQuerySpecification<T> _querySpecification;

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor, IExpressionParser expressionParser)
        {
            _objectQueryExecutor = objectQueryExecutor;
            _expressionParser = expressionParser;

            _querySpecification = new ObjectQuerySpecification<T>();
        }

        public StaticSelectBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            _querySpecification.QueryExpression = _expressionParser.Parse(queryExpression);
            return this;
        }

        public T Execute()
        {
            var result = ExecuteList();

            if (!result.Any())
            {
                throw new WeenyMapperException("No rows matched the given query");
            }

            return result.First();
        }

        public IList<T> ExecuteList()
        {
            return _objectQueryExecutor.Find(_querySpecification);
        }

        public StaticSelectBuilder<T> Select<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);

            _querySpecification.PropertiesToSelect.Add(propertyName);

            return this;
        }

        public void ExecuteAsync(Action<T> callback)
        {
            TaskRunner.Run(Execute, callback);
        }

        public void ExecuteListAsync(Action<IList<T>> callback)
        {
            TaskRunner.Run(ExecuteList, callback);
        }

        public void ExecuteScalarAsync<TScalar>(Action<TScalar> callback)
        {
            TaskRunner.Run(ExecuteScalar<TScalar>, callback);
        }

        public TScalar ExecuteScalar<TScalar>()
        {
            return _objectQueryExecutor.FindScalar<T, TScalar>(_querySpecification);
        }

        public void ExecuteScalarListAsync<TScalar>(Action<IList<TScalar>> callback)
        {
            TaskRunner.Run(ExecuteScalarList<TScalar>, callback);
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
    }
}
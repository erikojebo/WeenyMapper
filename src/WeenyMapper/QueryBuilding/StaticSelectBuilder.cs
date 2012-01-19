using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WeenyMapper.Async;
using WeenyMapper.Exceptions;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryBuilding
{
    public class StaticSelectBuilder<T> : StaticCommandBuilderBase<T> where T : new()
    {
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private readonly IExpressionParser _expressionParser;
        private readonly IList<string> _propertiesToSelect = new List<string>();
        private QueryExpression _parsedExpression = new RootExpression();

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor, IExpressionParser expressionParser)
        {
            _objectQueryExecutor = objectQueryExecutor;
            _expressionParser = expressionParser;
        }

        public StaticSelectBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            _parsedExpression = _expressionParser.Parse(queryExpression);
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
            if (_propertiesToSelect.Any())
            {
                return _objectQueryExecutor.Find<T>(typeof(T).Name, _parsedExpression, _propertiesToSelect);
            }

            return _objectQueryExecutor.Find<T>(typeof(T).Name, _parsedExpression);
        }

        public StaticSelectBuilder<T> Select<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);
            _propertiesToSelect.Add(propertyName);
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
            if (_propertiesToSelect.Any())
            {
                return _objectQueryExecutor.FindScalar<T, TScalar>(typeof(T).Name, _parsedExpression, _propertiesToSelect);
            }

            return _objectQueryExecutor.FindScalar<T, TScalar>(typeof(T).Name, _parsedExpression);
        }

        public void ExecuteScalarListAsync<TScalar>(Action<IList<TScalar>> callback)
        {
            TaskRunner.Run(ExecuteScalarList<TScalar>, callback);
        }

        public IList<TScalar> ExecuteScalarList<TScalar>()
        {
            if (_propertiesToSelect.Any())
            {
                return _objectQueryExecutor.FindScalarList<T, TScalar>(typeof(T).Name, _parsedExpression, _propertiesToSelect);
            }

            return _objectQueryExecutor.FindScalarList<T, TScalar>(typeof(T).Name, _parsedExpression);
        }
    }
}
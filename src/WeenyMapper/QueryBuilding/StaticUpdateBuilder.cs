using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.Async;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryBuilding
{
    public class StaticUpdateBuilder<T> : StaticCommandBuilderBase<T>
    {
        private readonly IObjectUpdateExecutor _objectUpdateExecutor;
        private readonly IExpressionParser _expressionParser;
        private readonly IDictionary<string, object> _setters = new Dictionary<string, object>();
        private QueryExpression _queryExpression = new RootExpression();

        public StaticUpdateBuilder(IObjectUpdateExecutor objectUpdateExecutor, IExpressionParser expressionParser)
        {
            _objectUpdateExecutor = objectUpdateExecutor;
            _expressionParser = expressionParser;
        }

        public int Update(T instance)
        {
            return _objectUpdateExecutor.Update(instance);
        }

        public StaticUpdateBuilder<T> Set<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _setters);
            return this;
        }

        public int Execute()
        {
            return _objectUpdateExecutor.Update<T>(_queryExpression, _setters);
        }

        public void UpdateAsync(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(() => Update(entity), callback, errorCallback);
        }

        public void ExecuteAsync(Action<int> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(Execute, callback, errorCallback);
        }

        public StaticUpdateBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            _queryExpression = _expressionParser.Parse(queryExpression);
            return this;
        }
    }
}
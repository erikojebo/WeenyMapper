using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.Async;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticUpdateBuilder<T> : StaticCommandBuilderBase<T>
    {
        private readonly IObjectUpdateExecutor _objectUpdateExecutor;
        private readonly IExpressionParser _expressionParser;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _setters = new Dictionary<string, object>();
        private QueryExpression _queryExpression;

        public StaticUpdateBuilder(IObjectUpdateExecutor objectUpdateExecutor, IExpressionParser expressionParser)
        {
            _objectUpdateExecutor = objectUpdateExecutor;
            _expressionParser = expressionParser;
        }

        public int Update(T instance)
        {
            return _objectUpdateExecutor.Update(instance);
        }

        public StaticUpdateBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _constraints);
            return this;
        }

        public StaticUpdateBuilder<T> Set<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _setters);
            return this;
        }

        public int Execute()
        {
            if (_queryExpression != null)
            {
                return _objectUpdateExecutor.Update<T>(_queryExpression, _setters);
            }

            return _objectUpdateExecutor.Update<T>(_constraints, _setters);
        }

        public void UpdateAsync(T entity, Action callback)
        {
            TaskRunner.Run(() => Update(entity), callback);
        }

        public void ExecuteAsync(Action<int> callback)
        {
            TaskRunner.Run(Execute, callback);
        }

        public StaticUpdateBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            _queryExpression = _expressionParser.Parse(queryExpression);
            return this;
        }
    }
}
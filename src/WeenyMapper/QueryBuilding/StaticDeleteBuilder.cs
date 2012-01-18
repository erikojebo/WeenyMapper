using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.Async;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryBuilding
{
    public class StaticDeleteBuilder<T> : StaticCommandBuilderBase<T>
    {
        private readonly ObjectDeleteExecutor _objectDeleteExecutor;
        private readonly ExpressionParser _expressionParser;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private QueryExpression _queryExpression;

        public StaticDeleteBuilder(ObjectDeleteExecutor objectDeleteExecutor, ExpressionParser expressionParser)
        {
            _objectDeleteExecutor = objectDeleteExecutor;
            _expressionParser = expressionParser;
        }

        public StaticDeleteBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _constraints);
            return this;
        }

        public int Execute()
        {
            if (_queryExpression != null)
            {
                return _objectDeleteExecutor.Delete<T>(_queryExpression);
            }

            return _objectDeleteExecutor.Delete<T>(_constraints);
        }

        public void ExecuteAsync(Action<int> callback)
        {
            TaskRunner.Run(Execute, callback);
        }

        public StaticDeleteBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            _queryExpression = _expressionParser.Parse(queryExpression);
            return this;
        }
    }
}
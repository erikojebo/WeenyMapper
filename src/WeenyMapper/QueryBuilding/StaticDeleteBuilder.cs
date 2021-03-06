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
        private readonly IObjectDeleteExecutor _objectDeleteExecutor;
        private readonly ExpressionParser _expressionParser;
        private QueryExpression _queryExpression = new RootExpression();

        public StaticDeleteBuilder(IObjectDeleteExecutor objectDeleteExecutor, ExpressionParser expressionParser)
        {
            _objectDeleteExecutor = objectDeleteExecutor;
            _expressionParser = expressionParser;
        }

        public int Execute()
        {
            return _objectDeleteExecutor.Delete<T>(_queryExpression);
        }

        public void ExecuteAsync(Action<int> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(Execute, callback, errorCallback);
        }

        public StaticDeleteBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            _queryExpression = _expressionParser.Parse(queryExpression);
            return this;
        }
    }
}
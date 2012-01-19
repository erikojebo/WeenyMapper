using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryBuilding
{
    public class StaticCountBuilder<T> : StaticCommandBuilderBase<T>
    {
        private readonly ObjectCountExecutor _objectCountExecutor;
        private readonly IExpressionParser _expressionParser;
        private QueryExpression _queryExpression = new RootExpression();

        public StaticCountBuilder(ObjectCountExecutor objectCountExecutor, IExpressionParser expressionParser)
        {
            _objectCountExecutor = objectCountExecutor;
            _expressionParser = expressionParser;
        }

        public StaticCountBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            _queryExpression = _expressionParser.Parse(expression);
            return this;
        }

        public int Execute()
        {
            return _objectCountExecutor.Count<T>(_queryExpression);
        }
    }
}
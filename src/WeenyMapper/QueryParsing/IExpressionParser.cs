using System;
using System.Linq.Expressions;

namespace WeenyMapper.QueryParsing
{
    public interface IExpressionParser
    {
        QueryExpression Parse<T>(Expression<Func<T, bool>> expression);
    }
}
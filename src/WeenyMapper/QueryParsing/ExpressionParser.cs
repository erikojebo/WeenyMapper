using System;
using System.Linq.Expressions;

namespace WeenyMapper.QueryParsing
{
    public class ExpressionParser
    {
        public QueryExpression Parse<T>(Expression<Func<T, bool>> expression)
        {
            // Member access
            // Constant

            var method = expression.Body as BinaryExpression;

            var left = method.Left as MemberExpression;
            var propertyName = left.Member.Name;

            var right = method.Right as ConstantExpression;
            var value = right.Value;
            return new AndExpression(new PropertyExpression(propertyName), new ValueExpression(value));
        }
    }
}
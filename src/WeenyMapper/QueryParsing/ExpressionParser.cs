using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using WeenyMapper.Exceptions;

namespace WeenyMapper.QueryParsing
{
    public class ExpressionParser
    {
        public QueryExpression Parse<T>(Expression<Func<T, bool>> expression)
        {
            return Parse(expression.Body);
        }

        private QueryExpression Parse(Expression expression)
        {
            if (expression is BinaryExpression)
            {
                return ParseBinaryExpression((BinaryExpression)expression);
            }
            if (expression is MemberExpression)
            {
                return ParseMemberExpression((MemberExpression)expression);
            }
            if (expression is ConstantExpression)
            {
                return ParseConstantExpression((ConstantExpression)expression);
            }
            if (expression is MethodCallExpression)
            {
                return ParseMethodCallExpression((MethodCallExpression)expression);
            }

            throw new WeenyMapperException("Invalid query expression");
        }

        private QueryExpression ParseBinaryExpression(BinaryExpression expression)
        {
            var left = Parse(expression.Left);
            var right = Parse(expression.Right);

            if (expression.NodeType == ExpressionType.Equal)
            {
                return new EqualsExpression(left, right);
            }
            if (expression.NodeType == ExpressionType.LessThan)
            {
                return new LessExpression(left, right);
            }
            if (expression.NodeType == ExpressionType.GreaterThan)
            {
                return new GreaterExpression(left, right);
            }
            if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return new GreaterOrEqualExpression(left, right);
            }
            if (expression.NodeType == ExpressionType.LessThanOrEqual)
            {
                return new LessOrEqualExpression(left, right);
            }
            if (expression.NodeType == ExpressionType.AndAlso)
            {
                return new AndExpression(left, right).Flatten();
            }
            if (expression.NodeType == ExpressionType.OrElse)
            {
                return new OrExpression(left, right).Flatten();
            }

            throw new WeenyMapperException("Unrecognized binary expression");
        }

        private QueryExpression ParseMemberExpression(MemberExpression expression)
        {
            if (expression.Member is PropertyInfo)
            {
                return new PropertyExpression(expression.Member.Name);
            }

            return CreateValueExpression(expression);
        }

        private QueryExpression ParseConstantExpression(ConstantExpression expression)
        {
            return new ValueExpression(expression.Value);
        }

        private QueryExpression ParseMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Contains")
            {
                return ParseContainsExpression(expression);
            }

            return CreateValueExpression(expression);
        }

        private QueryExpression ParseContainsExpression(MethodCallExpression expression)
        {
            var values = (IEnumerable<object>)Evaluate(expression.Arguments[0]);
            var propertyName = ((MemberExpression)expression.Arguments[1]).Member.Name;

            return new InExpression(new PropertyExpression(propertyName), new ArrayValueExpression(values));
        }

        private QueryExpression CreateValueExpression(Expression expression)
        {
            return new ValueExpression(Evaluate(expression));
        }

        private object Evaluate(Expression expression)
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }
    }
}
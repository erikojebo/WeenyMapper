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
            return CreateValueExpression(expression);
        }

        private QueryExpression CreateValueExpression(Expression expression)
        {
            return new ValueExpression(Evaluate(expression));
        }

        private object Evaluate(Expression expression)
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        //public class SyntaxTreeParser
        //{
        //    public static MethodCallInformation GetMethodCallInformation<T>(Expression<Action<T>> expression)
        //    {
        //        // Should never happen since the argument is an Expression<Action<T>>
        //        // which means that it is considered to have a void return type
        //        // and the expression "x => x.PropertyName" does not compile as an Action<T>
        //        // since a call to a property cannot be the only thing that is done in a
        //        // statement.
        //        if (!(expression.Body is MethodCallExpression))
        //        {
        //            throw new ArgumentException("Expression must be a method call");
        //        }

        //        var methodExpression = (MethodCallExpression)expression.Body;

        //        var argumentValues = methodExpression.Arguments
        //            .Select(x => Expression.Lambda(x).Compile().DynamicInvoke())
        //            .ToArray();

        //        return new MethodCallInformation
        //        {
        //            MethodName = methodExpression.Method.Name,
        //            Arguments = argumentValues
        //        };
        //    }
        //}

    }
}
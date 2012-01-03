using System;
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
            if (expression is BinaryExpression && expression.NodeType == ExpressionType.Equal)
            {
                return ParseEqualsExpression((BinaryExpression)expression);
            }
            if (expression is BinaryExpression && expression.NodeType == ExpressionType.AndAlso)
            {
                return ParseAndExpression((BinaryExpression)expression);
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

        private QueryExpression ParseEqualsExpression(BinaryExpression expression)
        {
            return new EqualsExpression(Parse(expression.Left), Parse(expression.Right));
        }

        private QueryExpression ParseAndExpression(BinaryExpression expression)
        {
            return new AndExpression(Parse(expression.Left), Parse(expression.Right)).Flatten();
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
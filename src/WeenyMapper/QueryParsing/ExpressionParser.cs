using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using WeenyMapper.Exceptions;

namespace WeenyMapper.QueryParsing
{
    public class ExpressionParser : IExpressionParser
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
            if (expression is NewExpression)
            {
                return ParseNewExpression((NewExpression)expression);
            }

            throw new WeenyMapperException("Invalid query expression");
        }

        private QueryExpression ParseBinaryExpression(BinaryExpression expression)
        {
            var left = Parse(expression.Left);
            var right = Parse(expression.Right);

            if (expression.NodeType == ExpressionType.AndAlso)
            {
                return new AndExpression(left, right).Flatten();
            }
            if (expression.NodeType == ExpressionType.OrElse)
            {
                return new OrExpression(left, right).Flatten();
            }

            PropertyExpression propertyExpression;
            ValueExpression valueExpression;

            if (left is PropertyExpression && right is ValueExpression)
            {
                propertyExpression = (PropertyExpression)left;
                valueExpression = (ValueExpression)right;
            }
            else if (left is ValueExpression && right is PropertyExpression)
            {
                propertyExpression = (PropertyExpression)right;
                valueExpression = (ValueExpression)left;
            }
            else
            {
                throw new WeenyMapperException("Invalid expression: An equals expression must have one property operand and one value operand");
            }
            
            if (expression.NodeType == ExpressionType.Equal)
            {
                return new EqualsExpression(propertyExpression, valueExpression);
            }
            if (expression.NodeType == ExpressionType.LessThan)
            {
                return new LessExpression(propertyExpression, valueExpression);
            }
            if (expression.NodeType == ExpressionType.GreaterThan)
            {
                return new GreaterExpression(propertyExpression, valueExpression);
            }
            if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return new GreaterOrEqualExpression(propertyExpression, valueExpression);
            }
            if (expression.NodeType == ExpressionType.LessThanOrEqual)
            {
                return new LessOrEqualExpression(propertyExpression, valueExpression);
            }

            throw new WeenyMapperException("Unrecognized binary expression");
        }

        private QueryExpression ParseNewExpression(NewExpression expression)
        {
            return CreateValueExpression(expression);
        }

        private QueryExpression ParseMemberExpression(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression && expression.Member is PropertyInfo)
            {
                return new ReflectedPropertyExpression((PropertyInfo)expression.Member);
            }

            return CreateValueExpression(expression);
        }

        private QueryExpression ParseConstantExpression(ConstantExpression expression)
        {
            return new ValueExpression(expression.Value);
        }

        private QueryExpression ParseMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Contains" && expression.Method.IsStatic)
            {
                return ParseContainsExpression(expression);
            }
            if (expression.Method.Name == "Contains" || expression.Method.Name == "StartsWith" || expression.Method.Name == "EndsWith")
            {
                return ParseLikeExpression(expression);
            }

            return CreateValueExpression(expression);
        }

        private QueryExpression ParseContainsExpression(MethodCallExpression expression)
        {
            var values = (IEnumerable<object>)Evaluate(expression.Arguments[0]);
            var propertyInfo = (PropertyInfo)((MemberExpression)expression.Arguments[1]).Member;

            return new InExpression(new ReflectedPropertyExpression(propertyInfo), new ArrayValueExpression(values));
        }

        private QueryExpression ParseLikeExpression(MethodCallExpression expression)
        {
            var searchString = (string)Evaluate(expression.Arguments[0]);

            var propertyExpression = Parse(expression.Object) as PropertyExpression;

            if (propertyExpression == null)
            {
                throw new WeenyMapperException("Failed to parse Like expression from String.Contains call");
            }

            return new LikeExpression(propertyExpression, searchString)
                {
                    HasStartingWildCard = expression.Method.Name != "StartsWith",
                    HasEndingWildCard = expression.Method.Name != "EndsWith"
                };
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
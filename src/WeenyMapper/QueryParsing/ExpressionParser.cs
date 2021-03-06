using System;
using System.Collections;
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
            if (expression is UnaryExpression)
            {
                return ParseUnaryExpression((UnaryExpression)expression);
            }
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

        private QueryExpression ParseUnaryExpression(UnaryExpression expression)
        {
            var inner = Parse(expression.Operand);

            if (expression.NodeType == ExpressionType.Not)
            {
                return new NotExpression(inner);
            }
            if (expression.NodeType == ExpressionType.Convert)
                return inner;

            throw new WeenyMapperException("Invalid expression: Invalid unary expression");
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
                throw new WeenyMapperException(
                    "Invalid expression: An equals expression must have one property operand and one value operand");
            }

            if (expression.NodeType == ExpressionType.Equal)
            {
                return new EqualsExpression(propertyExpression, valueExpression);
            }
            if (expression.NodeType == ExpressionType.NotEqual)
            {
                return new NotEqualExpression(propertyExpression, valueExpression);
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
            if (expression.Expression is MemberExpression &&
                ((MemberExpression)expression.Expression).Expression is ParameterExpression)
            {
                var referencePropertyExpression = (PropertyInfo)((MemberExpression)expression.Expression).Member;
                var dataPropertyExpression = (PropertyInfo)expression.Member;

                return new EntityReferenceExpression(referencePropertyExpression, dataPropertyExpression);
            }


            return CreateValueExpression(expression);
        }

        private QueryExpression ParseConstantExpression(ConstantExpression expression)
        {
            return new ValueExpression(expression.Value);
        }

        private QueryExpression ParseMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(string) &&
                (expression.Method.Name == "Contains" || expression.Method.Name == "StartsWith" ||
                 expression.Method.Name == "EndsWith"))
            {
                return ParseLikeExpression(expression);
            }
            if (expression.Method.Name == "Contains")
            {
                if (expression.Method.IsStatic)
                {
                    return ParseLinqContainsExpression(expression);
                }

                return ParseNonStaticContainsExpression(expression);
            }

            return CreateValueExpression(expression);
        }

        private QueryExpression ParseNonStaticContainsExpression(MethodCallExpression expression)
        {
            var values = (IEnumerable)Evaluate(expression.Object);
            var propertyInfo = (PropertyInfo)((MemberExpression)expression.Arguments[0]).Member;

            return new InExpression(new ReflectedPropertyExpression(propertyInfo), new ArrayValueExpression(values));
        }

        private QueryExpression ParseLinqContainsExpression(MethodCallExpression expression)
        {
            var values = (IEnumerable)Evaluate(expression.Arguments[0]);
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
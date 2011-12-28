using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WeenyMapper.Reflection
{
    public static class PropertyReader<TInstance>
    {
        public static string GetPropertyName<TValue>(Expression<Func<TInstance, TValue>> property)
        {
            return GetProperty(property).Name;
        }

        private static PropertyInfo GetProperty(Expression body)
        {
            if (body is LambdaExpression)
            {
                body = ((LambdaExpression)body).Body;
            }

            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (PropertyInfo)((MemberExpression)body).Member;
                case ExpressionType.Convert:
                    var conversionOperand = (MemberExpression)((UnaryExpression)body).Operand;
                    return (PropertyInfo)conversionOperand.Member;
                default:
                    throw new InvalidOperationException();
            }
        }

    }
}
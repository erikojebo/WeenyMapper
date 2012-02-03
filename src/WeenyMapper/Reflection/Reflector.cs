using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace WeenyMapper.Reflection
{
    public static class Reflector<TInstance>
    {
        public static string GetPropertyName<TValue>(Expression<Func<TInstance, TValue>> property)
        {
            return GetProperty(property).Name;
        }

        public static PropertyInfo GetProperty(Expression<Func<TInstance, object>> expression)
        {
            return GetProperty((Expression)expression);
        }

        public static PropertyInfo GetProperty(Expression body)
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

    public static class Reflector
    {
        public static IList CreateGenericList(Type itemType)
        {
            var listType = typeof(List<>);

            var typedListType = listType.MakeGenericType(itemType);

            var typedList = (IList)Activator.CreateInstance(typedListType);

            return typedList;
        }
    }
}
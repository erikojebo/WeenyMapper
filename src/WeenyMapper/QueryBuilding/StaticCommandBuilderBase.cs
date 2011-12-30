using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticCommandBuilderBase<T>
    {
        protected void StorePropertyValue<TValue>(Expression<Func<T, TValue>> getter, TValue value, IDictionary<string, object> propertyValues)
        {
            var propertyName = GetPropertyName(getter);
            propertyValues[propertyName] = value;
        }

        protected string GetPropertyName<TInstance, TValue>(Expression<Func<TInstance, TValue>> propertySelector)
        {
            return PropertyMetadataReader<TInstance>.GetPropertyName(propertySelector);
        }
    }
}
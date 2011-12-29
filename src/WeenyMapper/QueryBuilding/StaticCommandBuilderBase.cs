using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticCommandBuilderBase<T> {
        protected void StorePropertyValue<TValue>(Expression<Func<T, TValue>> getter, TValue value, IDictionary<string, object> propertyValues)
        {
            var propertyName = PropertyMetadataReader<T>.GetPropertyName(getter);
            propertyValues[propertyName] = value;
        }
    }
}
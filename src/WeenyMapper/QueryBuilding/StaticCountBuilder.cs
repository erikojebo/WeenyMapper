using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticCountBuilder<T>
    {
        private readonly ObjectCountExecutor _objectCountExecutor;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();

        public StaticCountBuilder(ObjectCountExecutor objectCountExecutor)
        {
            _objectCountExecutor = objectCountExecutor;
        }

        public StaticCountBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            var propertyName = PropertyMetadataReader<T>.GetPropertyName(getter);
            _constraints[propertyName] = value;
            return this;
        }

        public int Execute()
        {
            return _objectCountExecutor.Count<T>(_constraints);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticDeleteBuilder<T>
    {
        private readonly ObjectDeleteExecutor _objectDeleteExecutor;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();

        public StaticDeleteBuilder(ObjectDeleteExecutor objectDeleteExecutor)
        {
            _objectDeleteExecutor = objectDeleteExecutor;
        }

        public StaticDeleteBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            var propertyName = PropertyMetadataReader<T>.GetPropertyName(getter);
            _constraints[propertyName] = value;
            return this;
        }

        public void Execute()
        {
            _objectDeleteExecutor.Delete<T>(_constraints);
        }
    }
}
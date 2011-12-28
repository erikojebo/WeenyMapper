using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticUpdateBuilder<T>
    {
        private readonly IObjectUpdateExecutor _objectUpdateExecutor;
        private IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private IDictionary<string, object> _setters = new Dictionary<string, object>();

        public StaticUpdateBuilder(IObjectUpdateExecutor objectUpdateExecutor)
        {
            _objectUpdateExecutor = objectUpdateExecutor;
        }

        public void Update(T instance)
        {
            _objectUpdateExecutor.Update(instance);
        }

        public StaticUpdateBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            var propertyName = PropertyMetadataReader<T>.GetPropertyName(getter);
            _constraints[propertyName] = value;

            return this;
        }

        public StaticUpdateBuilder<T> Set<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            var propertyName = PropertyMetadataReader<T>.GetPropertyName(getter);
            _setters[propertyName] = value;

            return this;
        }

        public void Execute()
        {
            _objectUpdateExecutor.Update<T>(_constraints, _setters);
        }
    }
}
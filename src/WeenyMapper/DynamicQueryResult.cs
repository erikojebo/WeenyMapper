using System;
using System.Collections.Generic;

namespace WeenyMapper
{
    public class DynamicQueryResult
    {
        private readonly Dictionary<string, object> _values;

        public DynamicQueryResult(Dictionary<string, object> values)
        {
            _values = values;
        }

        public T As<T>() where T : new()
        {
            var instance = new T();
            var instanceType = typeof(T);

            foreach (var value in _values)
            {
                var property = instanceType.GetProperty(value.Key);
                property.SetValue(instance, value.Value, null);
            }

            return instance;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Exceptions;

namespace WeenyMapper.Mapping
{
    public class EntityMapper : IEntityMapper
    {
        private readonly IConvention _convention;

        public EntityMapper(IConvention convention)
        {
            _convention = convention;
        }

        public T CreateInstance<T>(IDictionary<string, object> dictionary) where T : new()
        {
            var values = dictionary.Select(x => new ColumnValue(x.Key, x.Value));
            return (T)CreateInstance(typeof(T), values);
        }

        public T CreateInstance<T>(IList<ColumnValue> columnValues)
        {
            return (T)CreateInstance(typeof(T), columnValues);
        }

        public object CreateInstance(Type type, IEnumerable<ColumnValue> columnValues)
        {
            try
            {
                return InternalCreateInstance(type, columnValues);
            }
            catch (MissingMethodException)
            {
                throw new MissingDefaultConstructorException(type);
            }
        }

        private object InternalCreateInstance(Type type, IEnumerable<ColumnValue> columnValues)
        {
            var instance = Activator.CreateInstance(type);

            foreach (var columnValue in columnValues)
            {
                var property = GetProperty(type, columnValue);
                property.SetValue(instance, columnValue.Value, null);
            }

            return instance;
        }

        private PropertyInfo GetProperty(Type type, ColumnValue columnValue)
        {
            var propertyInfo = type.GetProperty(columnValue.Name);

            if (propertyInfo == null)
            {
                throw new MissingPropertyException(type, columnValue.Name);
            }

            return propertyInfo;
        }
    }
}
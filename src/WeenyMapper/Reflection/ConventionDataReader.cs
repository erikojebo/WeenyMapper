using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;

namespace WeenyMapper.Reflection
{
    public class ConventionDataReader : IConventionDataReader
    {
        private readonly IConvention _convention;

        public ConventionDataReader(IConvention convention)
        {
            _convention = convention;
        }

        public IDictionary<string, object> GetColumnValuesFromEntity(object instance)
        {
            var propertyValues = GetPropertyValues(instance);

            var columnValues = new Dictionary<string, object>();

            foreach (var propertyValue in propertyValues)
            {
                var columnName = _convention.GetColumnName(propertyValue.Key);
                columnValues[columnName] = propertyValue.Value;
            }

            return columnValues;
        }

        public string GetTableName<T>()
        {
            return _convention.GetTableName(typeof(T).Name);
        }

        public string GetPrimaryKeyColumnName<T>()
        {
            var propertyName = GetIdProperty<T>().Name;
            return _convention.GetColumnName(propertyName);
        }

        public object GetPrimaryKeyValue<T>(T instance)
        {
            var property = GetIdProperty<T>();
            return property.GetValue(instance, null);
        }

        public IEnumerable<string> GetColumnNames(Type type)
        {
            return GetColumnProperties(type).Select(x => x.Name);
        }
        
        public IEnumerable<PropertyInfo> GetColumnProperties(Type type)
        {
            return type.GetProperties().Where(_convention.ShouldMapProperty);
        }

        private PropertyInfo GetIdProperty<T>()
        {
            return typeof(T).GetProperties()
                .First(x => _convention.IsIdProperty(x.Name));
        }

        public IDictionary<string, object> GetColumnValues(IDictionary<string, object> propertyValueMap)
        {
            return propertyValueMap.TransformKeys(_convention.GetColumnName);
        }

        private IDictionary<string, object> GetPropertyValues(object instance)
        {
            var properties = GetColumnProperties(instance.GetType());

            var propertyValues = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                propertyValues[property.Name] = property.GetValue(instance, null);
            }

            return propertyValues;
        }
    }
}
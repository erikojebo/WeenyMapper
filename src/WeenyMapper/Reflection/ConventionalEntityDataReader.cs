using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;

namespace WeenyMapper.Reflection
{
    public class ConventionalEntityDataReader : IConventionalEntityDataReader
    {
        private readonly IConvention _convention;

        public ConventionalEntityDataReader(IConvention convention)
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
            var idProperty = typeof(T).GetProperties()
                .Select(x => x.Name)
                .First(_convention.IsIdProperty);

            return _convention.GetColumnName(idProperty);
        }

        public IDictionary<string, object> GetColumnValues(IDictionary<string, object> propertyValueMap)
        {
            return propertyValueMap.TransformKeys(_convention.GetColumnName);
        }

        private IDictionary<string, object> GetPropertyValues(object instance)
        {
            var properties = instance.GetType().GetProperties();

            var propertyValues = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                propertyValues[property.Name] = property.GetValue(instance, null);
            }

            return propertyValues;
        }
    }
}
using System.Collections.Generic;
using WeenyMapper.Conventions;

namespace WeenyMapper.Reflection
{
    public class PropertyReader : IPropertyReader
    {
        private readonly IConvention _convention;

        public PropertyReader(IConvention convention)
        {
            _convention = convention;
        }

        public IDictionary<string, object> GetColumnValues(object instance)
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

        public IDictionary<string, object> GetPropertyValues(object instance)
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
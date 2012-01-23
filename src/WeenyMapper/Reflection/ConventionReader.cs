using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;

namespace WeenyMapper.Reflection
{
    public class ConventionReader : IConventionDataReader
    {
        private readonly IConvention _convention;

        public ConventionReader(IConvention convention)
        {
            _convention = convention;
        }

        public IDictionary<string, object> GetAllColumnValues(object instance)
        {
            var propertyValues = GetPropertyValues(instance);
            return propertyValues.TransformKeys(_convention.GetColumnName);
        }
        
        public IDictionary<string, object> GetColumnValuesForInsert(object instance)
        {
            var propertyValues = GetPropertyValues(instance);
            var entityType = instance.GetType();

            if (_convention.HasIdentityId(entityType))
            {
                propertyValues.Remove(GetIdPropertyName(entityType));
            }

            return propertyValues.TransformKeys(_convention.GetColumnName);
        }

        public string GetTableName<T>()
        {
            return _convention.GetTableName(typeof(T).Name);
        }

        public string GetPrimaryKeyColumnName<T>()
        {
            return GetPrimaryKeyColumnName(typeof(T));
        }

        public string GetPrimaryKeyColumnName(Type type)
        {
            var propertyName = GetIdPropertyName(type);
            return _convention.GetColumnName(propertyName);
        }

        private string GetIdPropertyName(Type type)
        {
            return GetIdProperty(type).Name;
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
            return GetIdProperty(typeof(T));
        }

        public PropertyInfo GetIdProperty(Type type)
        {
            return type.GetProperties()
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

        public string GetColumnName(string propertyName)
        {
            return _convention.GetColumnName(propertyName);
        }

        public string GetTableName(string className)
        {
            return _convention.GetTableName(className);
        }

        public bool IsIdProperty(string propertyName)
        {
            return _convention.IsIdProperty(propertyName);
        }

        public bool ShouldMapProperty(PropertyInfo propertyInfo)
        {
            return _convention.ShouldMapProperty(propertyInfo);
        }

        public bool HasIdentityId(Type entityType)
        {
            return _convention.HasIdentityId(entityType);
        }
    }
}
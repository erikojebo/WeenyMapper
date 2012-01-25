using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;

namespace WeenyMapper.Reflection
{
    public class ConventionReader : IConventionReader
    {
        private readonly IConvention _convention;

        public ConventionReader(IConvention convention)
        {
            _convention = convention;
        }

        public IDictionary<string, object> GetAllColumnValues(object instance)
        {
            var propertyValues = GetPropertyValues(instance);
            return propertyValues.TransformKeys(x => GetColumnName(x, instance.GetType()));
        }

        public IDictionary<string, object> GetColumnValuesForInsert(object instance)
        {
            var propertyValues = GetPropertyValues(instance);
            var entityType = instance.GetType();

            if (_convention.HasIdentityId(entityType))
            {
                propertyValues.Remove(GetIdPropertyName(entityType));
            }

            return propertyValues.TransformKeys(x => GetColumnName(x, instance.GetType()));
        }

        public string GetTableName<T>()
        {
            return _convention.GetTableName(typeof(T));
        }

        public string GetPrimaryKeyColumnName<T>()
        {
            return GetPrimaryKeyColumnName(typeof(T));
        }

        public string GetPrimaryKeyColumnName(Type type)
        {
            var propertyName = GetIdPropertyName(type);
            return GetColumnName(propertyName, type);
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
            return type.GetProperties().First(_convention.IsIdProperty);
        }

        public IDictionary<string, object> GetColumnValues<T>(IDictionary<string, object> propertyValueMap)
        {
            return propertyValueMap.TransformKeys(x => GetColumnName(x, typeof(T)));
        }

        private IDictionary<string, object> GetPropertyValues(object instance)
        {
            var properties = GetColumnProperties(instance.GetType());

            var propertyValues = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                if (IsEntityCollectionProperty(property))
                {
                    continue;
                }
                if (!IsDataProperty(property))
                {
                    continue;
                }

                propertyValues[property.Name] = property.GetValue(instance, null);

            }

            return propertyValues;
        }

        private bool IsDataProperty(PropertyInfo property)
        {
            var dataPropertyTypes = new List<Type>
                {
                    typeof(Guid),
                    typeof(DateTime),
                    typeof(TimeSpan),
                    typeof(string),
                    typeof(double),
                    typeof(decimal),
                    typeof(float),
                    typeof(bool),
                    typeof(char),
                    typeof(long),
                    typeof(ulong),
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(byte),
                    typeof(sbyte),
                };

            return dataPropertyTypes.Contains(property.PropertyType);
        }

        private bool IsEntityCollectionProperty(PropertyInfo property)
        {
            return property.PropertyType.ImplementsGenericInterface(typeof(IEnumerable<>)) && property.PropertyType != typeof(string);
        }

        public string GetColumnName(PropertyInfo propertyInfo)
        {
            return _convention.GetColumnName(propertyInfo);
        }

        public string GetColumnNamee<T>(string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            return _convention.GetColumnName(propertyInfo);
        }

        public void SetId(object entity, int id)
        {
            var property = GetIdProperty(entity.GetType());
            property.SetValue(entity, id, null);
        }

        public string GetColumnName(string propertyName, Type type)
        {
            var propertyInfo = type.GetProperty(propertyName);
            return _convention.GetColumnName(propertyInfo);
        }

        public string GetTableName(Type entityType)
        {
            return _convention.GetTableName(entityType);
        }

        public bool IsIdProperty(PropertyInfo propertyInfo)
        {
            return _convention.IsIdProperty(propertyInfo);
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
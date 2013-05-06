using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Exceptions;
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

        public IDictionary<string, object> GetColumnValuesForInsertOrUpdate(object instance)
        {
            var propertyValues = GetColumnValues(instance);
            var entityType = instance.GetType();

            if (_convention.HasIdentityId(entityType))
            {
                propertyValues.Remove(GetIdPropertyName(entityType));
            }

            return propertyValues;
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

        public object GetPrimaryKeyValue(object instance)
        {
            var property = GetIdProperty(instance.GetType());
            return property.GetValue(instance, null);
        }

        public IEnumerable<string> GetSelectableMappedPropertyNames(Type type)
        {
            return GetMappedProperties(type).Where(x => !IsEntityCollectionProperty(x)).Select(x => x.Name);
        }

        public IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return type.GetProperties().Where(_convention.ShouldMapProperty);
        }

        public PropertyInfo TryGetIdProperty(Type type)
        {
            return type.GetProperties().FirstOrDefault(_convention.IsIdProperty);
        }

        public PropertyInfo GetIdProperty(Type type)
        {
            var idProperty = TryGetIdProperty(type);

            if (idProperty == null)
                throw new WeenyMapperException("Could not find an id property in the type {0} using the convention {1}", type, _convention);

            return idProperty;
        }

        public bool HasIdProperty(Type type)
        {
            return TryGetIdProperty(type) != null;
        }

        public string TryGetPrimaryKeyColumnName(Type type)
        {
            if (HasIdProperty(type))
                return GetPrimaryKeyColumnName(type);

            return null;
        }

        public IDictionary<string, object> GetColumnValues<T>(IDictionary<PropertyInfo, object> propertyValueMap)
        {
            var columnValues = new Dictionary<string, object>();

            foreach (var propertyValue in propertyValueMap)
            {
                var propertyInfo = propertyValue.Key;
                var value = propertyValue.Value;

                if (IsEntityReferenceProperty(propertyInfo))
                {
                    var foreignKeyColumnName = GetManyToOneForeignKeyColumnName(propertyInfo);
                    var referencedEntityId = GetPrimaryKeyValue(value);

                    columnValues[foreignKeyColumnName] = referencedEntityId;
                }
                else
                {
                    var columnName = GetColumnName(propertyInfo);
                    columnValues[columnName] = value;
                }
            }

            return columnValues;
        }

        public IDictionary<string, object> GetColumnValues(object instance)
        {
            var properties = GetMappedProperties(instance.GetType()).ToList();

            var propertyValues = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                var columnName = GetColumnName(property);
                var value = property.GetValue(instance, null);

                if (IsEntityCollectionProperty(property))
                {
                    continue;
                }
                if (IsEntityReferenceProperty(property))
                {
                    columnName = _convention.GetManyToOneForeignKeyColumnName(property);

                    value = GetForeignKeyValue(instance, properties, columnName, value);
                }

                propertyValues[columnName] = value;
            }

            return propertyValues;
        }

        private object GetForeignKeyValue(object instance, List<PropertyInfo> properties, string columnName, object value)
        {
            var foreignKeyIdProperty = properties.FirstOrDefault(x => GetColumnName(x) == columnName);
            var foreignKeyIdPropertyValue = foreignKeyIdProperty != null ? foreignKeyIdProperty.GetValue(instance, null) : null;

            // The property is a navigation property so the value is another entity instance. Hence, the foreign key value
            // is then actually the primary key of the other entity.
            if (value != null)
            {
                value = GetPrimaryKeyValue(value);
            }
            // If the navigation property is null, check if there is an explicit property for the foreign key id. If so,
            // use that value instead.
            else if (foreignKeyIdPropertyValue != null)
            {
                value = foreignKeyIdPropertyValue;
            }
            // If neither navigation property or explicit id property is set to something, then write a null to the database
            else
            {
                value = DBNull.Value;
            }
            return value;
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

            var isPrimitiveType = dataPropertyTypes.Contains(property.PropertyType);
            var isNullableType = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

            return isPrimitiveType || isNullableType || property.PropertyType.IsEnum;
        }

        private bool IsEntityCollectionProperty(PropertyInfo property)
        {
            return property.PropertyType.ImplementsGenericInterface(typeof(IEnumerable<>)) && property.PropertyType != typeof(string);
        }

        private bool IsEntityReferenceProperty(PropertyInfo property)
        {
            return !IsDataProperty(property) && !IsEntityCollectionProperty(property);
        }

        public string GetColumnName(PropertyInfo propertyInfo)
        {
            return _convention.GetColumnName(propertyInfo);
        }

        public string GetColumnName<T>(string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);

            if (!IsDataProperty(propertyInfo))
            {
                return GetManyToOneForeignKeyColumnName(propertyInfo);
            }

            return _convention.GetColumnName(propertyInfo);
        }

        public void SetId(object entity, int id)
        {
            var property = GetIdProperty(entity.GetType());
            property.SetValue(entity, id, null);
        }

        public IEnumerable<string> GetSelectableColumNames(Type type)
        {
            var mappedProperties = GetMappedProperties(type);

            var entityReferenceProperties = mappedProperties.Where(IsEntityReferenceProperty);
            var dataProperties = mappedProperties.Where(IsDataProperty);

            var foreignKeyColumnNames = entityReferenceProperties.Select(GetManyToOneForeignKeyColumnName);
            var dataColumnNames = dataProperties.Select(GetColumnName);

            return dataColumnNames.Concat(foreignKeyColumnNames).Distinct();
        }

        public PropertyInfo GetPropertyForColumn(string columnName, Type type)
        {
            var properties = GetMappedProperties(type);
            return properties.FirstOrDefault(x => GetColumnName(x) == columnName);
        }

        public bool IsForeignKey(string columnName, Type type)
        {
            var properties = GetMappedProperties(type);
            return properties.Any(x => IsEntityReferenceProperty(x) && GetManyToOneForeignKeyColumnName(x) == columnName);
        }

        public bool HasProperty(string columnName, Type type)
        {
            return GetPropertyForColumn(columnName, type) != null;
        }

        public bool IsEntityReferenceProperty(string columnName, Type type)
        {
            return IsForeignKey(columnName, type) && !HasProperty(columnName, type);
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

        public string GetManyToOneForeignKeyColumnName(PropertyInfo propertyInfo)
        {
            return _convention.GetManyToOneForeignKeyColumnName(propertyInfo);
        }
    }
}
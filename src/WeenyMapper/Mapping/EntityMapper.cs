using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Exceptions;
using WeenyMapper.Reflection;

namespace WeenyMapper.Mapping
{
    public class EntityMapper : IEntityMapper
    {
        private readonly IConventionReader _conventionReader;

        public EntityMapper(IConventionReader conventionReader)
        {
            _conventionReader = conventionReader;
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

        public T CreateInstance<T>(IList<ColumnValue> columnValues, ObjectRelation relation)
        {
            return (T)CreateInstance(typeof(T), columnValues, relation);
        }

        public object CreateInstance(Type type, IEnumerable<ColumnValue> columnValues, ObjectRelation relation = null)
        {
            try
            {
                return InternalCreateInstance(type, columnValues, relation);
            }
            catch (MissingMethodException)
            {
                throw new MissingDefaultConstructorException(type);
            }
        }

        private object InternalCreateInstance(Type type, IEnumerable<ColumnValue> columnValues, ObjectRelation relation)
        {
            if (relation == null)
            {
                return InternalCreateInstance(type, columnValues);
            }

            var child = InternalCreateInstance(relation.ChildProperty.DeclaringType, columnValues);
            var parent = InternalCreateInstance(relation.ParentProperty.DeclaringType, columnValues);

            var hasParentProperties = columnValues.Any(x => IsForType(x, relation.ParentProperty.DeclaringType));
            if (hasParentProperties)
            {
                relation.ChildProperty.SetValue(child, parent, null);
            }

            return child;
        }

        private object InternalCreateInstance(Type type, IEnumerable<ColumnValue> columnValues)
        {
            var instance = Activator.CreateInstance(type);

            var columnValuesForCurrentType = columnValues.Where(x => IsForType(x, type));
            foreach (var columnValue in columnValuesForCurrentType)
            {
                var property = GetProperty(type, columnValue);
                property.SetValue(instance, columnValue.Value, null);
            }

            return instance;
        }

        private bool IsForType(ColumnValue columnValue, Type type)
        {
            var isTableQualifiedColumnName = IsTableQualifiedColumnName(columnValue.Name);

            return !isTableQualifiedColumnName || columnValue.Name.StartsWith(type.Name);
        }

        private PropertyInfo GetProperty(Type type, ColumnValue columnValue)
        {
            string columnName = GetColumnName(columnValue);

            var propertyInfo = _conventionReader.GetPropertyForColumn(columnName, type);

            if (propertyInfo == null)
            {
                throw MissingPropertyException.CreateFromColumnName(type, columnName);
            }

            return propertyInfo;
        }

        private string GetColumnName(ColumnValue columnValue)
        {
            var columnName = columnValue.Name;

            if (IsTableQualifiedColumnName(columnName))
            {
                columnName = StripTableName(columnName);
            }
            return columnName;
        }

        private string StripTableName(string columnName)
        {
            return columnName.Substring(columnName.IndexOf(" ") + 1);
        }

        private bool IsTableQualifiedColumnName(string columnName)
        {
            return columnName.Contains(" ");
        }
    }
}
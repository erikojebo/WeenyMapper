using System;
using System.Collections;
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
            return (T)CreateInstance(typeof(T), new Row(values));
        }

        public T CreateInstance<T>(Row columnValues)
        {
            return (T)CreateInstance(typeof(T), columnValues);
        }

        public T CreateInstanceGraph<T>(Row row, ObjectRelation relation)
        {
            return (T)CreateInstanceGraph(typeof(T), row, relation);
        }

        private object CreateInstanceGraph(Type type, Row row, ObjectRelation relation)
        {
            return CreateInstanceGraph(type, row.ColumnValues, relation);
        }

        private object CreateInstanceGraph(Type type, IEnumerable<ColumnValue> columnValues, ObjectRelation relation)
        {
            var child = CreateInstance(relation.ChildProperty.DeclaringType, new Row(columnValues));
            var parent = CreateInstance(relation.ParentProperty.DeclaringType, new Row(columnValues));

            var hasParentProperties = columnValues.Any(x => x.IsForType(relation.ParentProperty.DeclaringType, _conventionReader));
            if (hasParentProperties)
            {
                relation.ChildProperty.SetValue(child, parent, null);
                var parentsChildCollection = (IList)relation.ParentProperty.GetValue(parent, null);
                parentsChildCollection.Add(child);
            }

            return child;
        }

        public object CreateInstance(Type type, Row columnValues)
        {
            var instance = CreateInstance(type);

            var columnValuesForCurrentType = columnValues.ColumnValues.Where(x => x.IsForType(type, _conventionReader));
            foreach (var columnValue in columnValuesForCurrentType)
            {
                var property = GetProperty(type, columnValue);
                property.SetValue(instance, columnValue.Value, null);
            }

            return instance;
        }

        private object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception)
            {
                throw new MissingDefaultConstructorException(type);
            }
        }

        private PropertyInfo GetProperty(Type type, ColumnValue columnValue)
        {
            var propertyInfo = _conventionReader.GetPropertyForColumn(columnValue.ColumnName, type);

            if (propertyInfo == null)
            {
                throw MissingPropertyException.CreateFromColumnName(type, columnValue.ColumnName);
            }

            return propertyInfo;
        }
    }
}
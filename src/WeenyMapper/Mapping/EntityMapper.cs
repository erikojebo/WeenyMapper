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
            return (T)CreateInstance(typeof(T), new Row(values), new EntityCache(_conventionReader));
        }

        public T CreateInstance<T>(Row columnValues)
        {
            return (T)CreateInstance(typeof(T), columnValues, new EntityCache(_conventionReader));
        }

        public T CreateInstanceGraph<T>(Row row, ObjectRelation relation)
        {
            return CreateInstanceGraph<T>(row, new[] { relation });
        }

        public T CreateInstanceGraph<T>(Row row, IEnumerable<ObjectRelation> objectRelations)
        {
            return (T)CreateInstanceGraph(typeof(T), row, objectRelations, new EntityCache(_conventionReader));
        }

        public IList<T> CreateInstanceGraphs<T>(ResultSet resultSet)
        {
            var entityCache = new EntityCache(_conventionReader);
            var objects = new List<object>();

            foreach (var row in resultSet.Rows)
            {
                var instance = CreateInstance(typeof(T), row, entityCache);

                objects.Add(instance);
            }

            return objects.OfType<T>().Distinct(new IdPropertyComparer<T>(_conventionReader)).ToList();
        }

        public IList<T> CreateInstanceGraphs<T>(ResultSet resultSet, ObjectRelation parentChildRelation)
        {
            return CreateInstanceGraphs<T>(resultSet, new[] { parentChildRelation });
        }

        public IList<T> CreateInstanceGraphs<T>(ResultSet resultSet, IEnumerable<ObjectRelation> parentChildRelation)
        {
            var entityCache = new EntityCache(_conventionReader);
            var objects = new List<object>();

            foreach (var row in resultSet.Rows)
            {
                var instance = CreateInstanceGraph(typeof(T), row, parentChildRelation, entityCache);

                objects.Add(instance);
            }

            return objects.OfType<T>().Distinct(new IdPropertyComparer<T>(_conventionReader)).ToList();
        }

        private object CreateInstanceGraph(Type resultType, Row row, IEnumerable<ObjectRelation> relations, EntityCache entityCache)
        {
            object rootObject = null;

            foreach (var relation in relations)
            {
                var instance = CreateInstanceGraph(resultType, row, relation, entityCache);

                rootObject = rootObject ?? instance;
            }

            return rootObject;
        }

        private object CreateInstanceGraph(Type resultType, Row row, ObjectRelation relation, EntityCache entityCache)
        {
            var childType = relation.ChildProperty.DeclaringType;
            var parentType = relation.ParentProperty.DeclaringType;

            var child = CreateInstance(childType, row, entityCache);
            var parent = CreateInstance(parentType, row, entityCache);

            ConnectEntities(relation, child, parent);

            return resultType == childType ? child : parent;
        }

        private void ConnectEntities(ObjectRelation relation, object child, object parent)
        {
            relation.ChildProperty.SetValue(child, parent, null);
            var parentsChildCollection = (IList)relation.ParentProperty.GetValue(parent, null);

            var parentCollectionContainsChild = parentsChildCollection
                .OfType<object>()
                .Contains(child, new IdPropertyComparer<object>(_conventionReader));
            
            if (!parentCollectionContainsChild)
            {
                parentsChildCollection.Add(child);
            }
        }

        private object CreateInstance(Type type, Row row, EntityCache entityCache)
        {
            var instance = CreateInstance(type);

            var columnValuesForCurrentType = row.GetColumnValuesForType(type, _conventionReader)
                .Where(x => !_conventionReader.IsForeignKey(x.ColumnName, type))
                .ToList();

            foreach (var columnValue in columnValuesForCurrentType)
            {
                var property = GetProperty(type, columnValue);
                property.SetValue(instance, columnValue.Value, null);
            }

            if (entityCache.Contains(instance))
            {
                return entityCache.GetExisting(instance);
            }

            entityCache.Add(instance);

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

        private class EntityCache
        {
            private readonly IDictionary<Type, IList<object>> _cache = new Dictionary<Type, IList<object>>();
            private readonly IConventionReader _conventionReader;
            private readonly IdPropertyComparer<object> _idPropertyComparer;

            public EntityCache(IConventionReader conventionReader)
            {
                _conventionReader = conventionReader;
                _idPropertyComparer = new IdPropertyComparer<object>(_conventionReader);
            }

            public bool Contains(object entity)
            {
                return GetCacheForType(entity).Contains(entity, _idPropertyComparer);
            }

            public void Add(object entity)
            {
                GetCacheForType(entity).Add(entity);
            }

            public object GetExisting(object entity)
            {
                return GetCacheForType(entity).First(x => _idPropertyComparer.Equals(x, entity));
            }

            private IList<object> GetCacheForType(object entity)
            {
                var entityType = entity.GetType();

                if (!_cache.ContainsKey(entityType))
                {
                    return _cache[entityType] = new List<object>();
                }

                return _cache[entityType];
            }
        }
    }
}
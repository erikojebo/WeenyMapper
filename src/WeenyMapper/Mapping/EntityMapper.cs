using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Exceptions;
using WeenyMapper.Extensions;
using WeenyMapper.Reflection;

namespace WeenyMapper.Mapping
{
    public class EntityMapper : IEntityMapper
    {
        public const string WeenyMapperGeneratedColumnNamePrefix = "WeenyMapperGenerated_";

        private readonly IConventionReader _conventionReader;
        private readonly EntityCache _entityCache;

        public EntityMapper(IConventionReader conventionReader)
        {
            _conventionReader = conventionReader;
            _entityCache = new EntityCache(_conventionReader);
        }

        public bool IsEntityCachingEnabled { get; set; }

        public T CreateInstance<T>(IDictionary<string, object> dictionary) where T : new()
        {
            var values = dictionary.Select(x => new ColumnValue(x.Key, x.Value));
            return (T)CreateInstance(typeof(T), new Row(values), GetEntityCache());
        }

        private EntityCache GetEntityCache()
        {
            if (IsEntityCachingEnabled)
            {
                return _entityCache;
            }

            return new EntityCache(_conventionReader);
        }

        public T CreateInstance<T>(Row columnValues)
        {
            return (T)CreateInstance(typeof(T), columnValues, GetEntityCache());
        }

        public T CreateInstanceGraph<T>(Row row, ObjectRelation relation)
        {
            return CreateInstanceGraph<T>(row, new[] { relation });
        }

        public T CreateInstanceGraph<T>(Row row, IEnumerable<ObjectRelation> objectRelations)
        {
            return (T)CreateInstanceGraph(typeof(T), row, objectRelations, GetEntityCache(), null).Entity;
        }

        public IList<T> CreateInstanceGraphs<T>(ResultSet resultSet)
        {
            var entityCache = GetEntityCache();

            var objects = resultSet.Rows.Select(row => CreateInstance(typeof(T), row, entityCache)).ToList();

            return GetDistinctResult<T>(resultSet, objects);
        }

        private IList<T> GetDistinctResult<T>(ResultSet resultSet, IEnumerable<CreatedEntity> createdEntities, string primaryAlias = null)
        {
            // If there are any objects that matched a given primary alias in the query, then only those objects
            // should be considered when for the return result. I.e. if a user does a join with Employee and Employee
            // to get a manager with all its subordinates, and specifies the primary alias "manager", then only the
            // actual instances that are managers should be returned, not those that are only subordinates and not managers.
            var createdEntitiesList = createdEntities.ToList();

            var primaryEntities = createdEntitiesList.Where(x => x.IsPrimaryEntityInQuery).Select(x => x.Entity).ToList();

            if (primaryEntities.Any())
                return GetDistinctResult<T>(resultSet, primaryEntities, primaryAlias);

            return GetDistinctResult<T>(resultSet, createdEntitiesList.Select(x => x.Entity), primaryAlias);
        }

        private IList<T> GetDistinctResult<T>(ResultSet resultSet, IEnumerable<object> objects, string primaryAlias = null)
        {
            var queryIncludesPrimaryKeyColumn = QueryIncludesPrimaryKeyColumn(typeof(T), resultSet, primaryAlias);

            // If the query does not include the primary key column all entities will have the default
            // value as id value, so trying to make the result set distinct on the id value would
            // always just return one single entity.
            if (queryIncludesPrimaryKeyColumn)
            {
                var comparer = GetEqualityComparer<T>();

                return objects.OfType<T>().Distinct(comparer).ToList();
            }

            return objects.OfType<T>().ToList();
        }

        private bool QueryIncludesPrimaryKeyColumn(Type type, ResultSet resultSet, string primaryAlias)
        {
            if (resultSet.Rows.IsEmpty())
                return false;

            return QueryIncludesPrimaryKeyColumn(type, resultSet.Rows.First(), primaryAlias);
        }

        private bool QueryIncludesPrimaryKeyColumn(Type type, Row row, string primaryAlias)
        {
            var primaryKeyColumnName = _conventionReader.TryGetPrimaryKeyColumnName(type);

            var typeHasIdProperty = primaryKeyColumnName != null;
            
            IList<ColumnValue> columnValuesForType;

            if (primaryAlias.IsNullOrWhiteSpace())
                columnValuesForType = row.GetColumnValuesForType(type, _conventionReader);
            else
                columnValuesForType = row.GetColumnValuesForAlias(primaryAlias);

            var resultIncludesPrimaryKeyColumn = columnValuesForType.Any(x => x.ColumnName == primaryKeyColumnName);

            return typeHasIdProperty && resultIncludesPrimaryKeyColumn;
        }


        private IEqualityComparer<T> GetEqualityComparer<T>()
        {
            return GetEqualityComparer<T>(typeof(T));
        }

        private IEqualityComparer<T> GetEqualityComparer<T>(Type type)
        {
            if (_conventionReader.HasIdProperty(type))
            {
                return new IdPropertyComparer<T>(_conventionReader);
            }

            return new EqualsEqualityComparer<T>();
        }

        public IList<T> CreateInstanceGraphs<T>(ResultSet resultSet, ObjectRelation parentChildRelation)
        {
            return CreateInstanceGraphs<T>(resultSet, new[] { parentChildRelation });
        }

        public IList<T> CreateInstanceGraphs<T>(ResultSet resultSet, IEnumerable<ObjectRelation> parentChildRelations, string primaryAlias = null)
        {
            var entityCache = GetEntityCache();
            var objects = new List<CreatedEntity>();

            var objectRelations = parentChildRelations.ToList();

            foreach (var row in resultSet.Rows)
            {
                var instance = CreateInstanceGraph(typeof(T), row, objectRelations, entityCache, primaryAlias);

                objects.Add(instance);
            }

            return GetDistinctResult<T>(resultSet, objects, primaryAlias);
        }

        private CreatedEntity CreateInstanceGraph(Type resultType, Row row, IEnumerable<ObjectRelation> relations, EntityCache entityCache, string primaryAlias)
        {
            CreatedEntity rootObject = null;

            var relationsList = relations.ToList();

            MovePrimaryRelationToFront(primaryAlias, relationsList);

            foreach (var relation in relationsList)
            {
                var instance = CreateInstanceGraph(resultType, row, relation, entityCache, primaryAlias);

                rootObject = rootObject ?? instance;
            }

            return rootObject;
        }

        private static void MovePrimaryRelationToFront(string primaryAlias, List<ObjectRelation> relationsList)
        {
            var relationWithPrimaryAlias = relationsList.FirstOrDefault(x => Matches(x.ChildAlias, primaryAlias) || Matches(x.ParentAlias, primaryAlias));

            if (relationWithPrimaryAlias != null)
            {
                relationsList.Remove(relationWithPrimaryAlias);
                relationsList.Insert(0, relationWithPrimaryAlias);
            }
        }

        private CreatedEntity CreateInstanceGraph(Type resultType, Row row, ObjectRelation relation, EntityCache entityCache, string primaryAlias)
        {
            var childType = relation.ChildType;
            var parentType = relation.ParentType;

            var child = CreateInstance(childType, row, entityCache, relation.ChildAlias);
            var parent = CreateInstance(parentType, row, entityCache, relation.ParentAlias);

            ConnectEntities(relation, child, parent);

            if (Matches(relation.ChildAlias, primaryAlias))
                return CreatedEntity.Primary(child);
            if (Matches(relation.ParentAlias, primaryAlias))
                return CreatedEntity.Primary(parent);

            return resultType == childType ? CreatedEntity.Ordinary(child) : CreatedEntity.Ordinary(parent);
        }

        private static bool Matches(string actualAlias, string primaryAlias)
        {
            if (primaryAlias.IsNullOrWhiteSpace() || actualAlias.IsNullOrWhiteSpace())
                return false;

            return actualAlias.ToLower() == primaryAlias.ToLower();
        }

        private void ConnectEntities(ObjectRelation relation, object child, object parent)
        {
            if (child == null || parent == null)
                return;

            if (relation.HasChildProperty)
                MapParentPropertyForChild(relation, child, parent);

            if (relation.HasParentProperty)
                MapChildCollectionForParent(relation, child, parent);
        }

        private static void MapParentPropertyForChild(ObjectRelation relation, object child, object parent)
        {
            relation.ChildProperty.SetValue(child, parent, null);
        }

        private void MapChildCollectionForParent(ObjectRelation relation, object child, object parent)
        {
            var parentsChildCollection = (IList)relation.ParentProperty.GetValue(parent, null);

            if (parentsChildCollection == null)
            {
                var list = Reflector.CreateGenericList(child.GetType());
                parentsChildCollection = list;
                relation.ParentProperty.SetValue(parent, list, null);
            }

            var comparer = GetEqualityComparer<object>(child.GetType());

            var parentCollectionContainsChild = parentsChildCollection
                .OfType<object>()
                .Contains(child, comparer);

            if (!parentCollectionContainsChild)
            {
                parentsChildCollection.Add(child);
            }
        }

        private object CreateInstance(Type type, Row row, EntityCache entityCache, string alias = null)
        {
            var instance = CreateInstance(type);

            IList<ColumnValue> columnValuesForType;

            if (String.IsNullOrWhiteSpace(alias))
            {
                columnValuesForType = row.GetColumnValuesForType(type, _conventionReader);                
            }
            else
            {
                columnValuesForType = row.GetColumnValuesForAlias(alias);
            }

            var columnValuesForCurrentType = columnValuesForType
                                                .Where(x => !_conventionReader.IsEntityReferenceProperty(x.ColumnName, type))
                                                .ToList();

            if (columnValuesForCurrentType.Any() && columnValuesForCurrentType.All(x => x.Value == null))
            {
                return null;
            }

            foreach (var columnValue in columnValuesForCurrentType)
            {
                var property = GetProperty(type, columnValue);
                property.SetValue(instance, columnValue.Value, null);
            }

            var includesPrimaryKeyColumn = QueryIncludesPrimaryKeyColumn(type, row, alias);

            // If the query does not include the primary key column we can't know if 
            // the cache already contains the entity or not, since we do not have anything
            // unique to match the entities on. So, treat all rows as different to make sure
            // no rows are incorrectly merged into one, which would be the case if we assumed
            // that they represented the same entity.
            if (entityCache.Contains(instance) && includesPrimaryKeyColumn)
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
                var comparer = GetComparer(entity);
                return GetCacheForType(entity).Contains(entity, comparer);
            }

            public void Add(object entity)
            {
                GetCacheForType(entity).Add(entity);
            }

            public object GetExisting(object entity)
            {
                var comparer = GetComparer(entity);
                return GetCacheForType(entity).First(x => comparer.Equals(x, entity));
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

            private IEqualityComparer<object> GetComparer(object entity)
            {
                if (_conventionReader.HasIdProperty(entity.GetType()))
                    return _idPropertyComparer;

                return new EqualsEqualityComparer<object>();
            }
        }

        private class CreatedEntity
        {
            public bool IsPrimaryEntityInQuery { get; private set; }
            public object Entity { get; private set; }

            public static CreatedEntity Primary(object entity)
            {
                return new CreatedEntity { IsPrimaryEntityInQuery = true, Entity = entity };
            }
            
            public static CreatedEntity Ordinary(object entity)
            {
                return new CreatedEntity { IsPrimaryEntityInQuery = false, Entity = entity };
            }
        }
    }
}
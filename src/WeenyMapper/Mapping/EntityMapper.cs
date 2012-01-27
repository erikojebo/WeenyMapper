using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;

namespace WeenyMapper.Mapping
{
    public class EntityMapper : IEntityMapper
    {
        private readonly IConvention _convention;

        public EntityMapper(IConvention convention)
        {
            _convention = convention;
        }

        public T CreateInstance<T>(IDictionary<string, object> dictionary) where T : new()
        {
            var propertiesInTargetType = typeof(T).GetProperties();

            var instance = new T();

            var matchingProperties = FindMatchingProperties(dictionary, typeof(T));

            foreach (var value in matchingProperties)
            {
                var property = propertiesInTargetType.First(x => _convention.GetColumnName(x) == GetColumnName(value.Key));
                property.SetValue(instance, value.Value, null);
            }

            return instance;
        }

        private string GetColumnName(string key)
        {
            if (key.Contains(" "))
            {
                return key.Substring(key.IndexOf(" ") + 1);
            }

            return key;
        }

        private IEnumerable<KeyValuePair<string, object>> FindMatchingProperties(IEnumerable<KeyValuePair<string, object>> dictionary, Type type)
        {
            return dictionary.Where(x => !x.Key.Contains(" ") || (x.Key.Contains(" ") && x.Key.StartsWith(_convention.GetTableName(type))));
        }

        public T CreateInstance<T>(Dictionary<string, object> dictionary, PropertyInfo[] propertyInfos) where T : new()
        {
            var instance = CreateInstance<T>(dictionary);

            propertyInfos.First().SetValue(instance, Activator.CreateInstance(propertyInfos.First().PropertyType), null);

            return instance;
        }
    }
}
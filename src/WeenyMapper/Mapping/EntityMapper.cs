using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var value in dictionary)
            {
                var property = propertiesInTargetType.First(x => _convention.GetColumnName(x) == value.Key);
                property.SetValue(instance, value.Value, null);
            }

            return instance;
        }
    }
}
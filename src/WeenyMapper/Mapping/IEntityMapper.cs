using System.Collections.Generic;

namespace WeenyMapper.Mapping
{
    public interface IEntityMapper
    {
        T CreateInstance<T>(IDictionary<string, object> dictionary) where T : new();
    }
}
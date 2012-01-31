using System.Collections.Generic;

namespace WeenyMapper.Mapping
{
    public interface IEntityMapper
    {
        T CreateInstance<T>(IDictionary<string, object> dictionary) where T : new();
        IList<T> CreateInstanceGraphs<T>(ResultSet resultSet);
        IList<T> CreateInstanceGraphs<T>(ResultSet resultSet, ObjectRelation parentChildRelation);
        IList<T> CreateInstanceGraphs<T>(ResultSet resultSet, IEnumerable<ObjectRelation> parentChildRelation);
    }
}
using System.Collections.Generic;

namespace WeenyMapper.Reflection
{
    public interface IConventionalEntityDataReader
    {
        IDictionary<string, object> GetColumnValuesFromEntity(object instance);
        IDictionary<string, object> GetColumnValues(IDictionary<string, object> propertyValueMap);
        string GetTableName<T>();
        string GetPrimaryKeyColumnName<T>();
    }
}
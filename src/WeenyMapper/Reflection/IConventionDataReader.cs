using System;
using System.Collections.Generic;
using WeenyMapper.Conventions;

namespace WeenyMapper.Reflection
{
    public interface IConventionDataReader : IConvention
    {
        IDictionary<string, object> GetAllColumnValues(object instance);
        IDictionary<string, object> GetColumnValues(IDictionary<string, object> propertyValueMap);
        string GetTableName<T>();
        string GetPrimaryKeyColumnName<T>();
        object GetPrimaryKeyValue<T>(T instance);
        IEnumerable<string> GetColumnNames(Type type);
        IDictionary<string, object> GetColumnValuesForInsert(object instance);
    }
}
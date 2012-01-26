using System;
using System.Collections.Generic;
using WeenyMapper.Conventions;

namespace WeenyMapper.Reflection
{
    public interface IConventionReader : IConvention
    {
        IDictionary<string, object> GetAllColumnValues(object instance);
        IDictionary<string, object> GetColumnValues<T>(IDictionary<string, object> propertyValueMap);
        string GetTableName<T>();
        string GetPrimaryKeyColumnName<T>();
        object GetPrimaryKeyValue(object instance);
        IEnumerable<string> GetSelectableMappedPropertyNames(Type type);
        IDictionary<string, object> GetColumnValuesForInsert(object instance);
        string GetColumnName<T>(string propertyName);
        void SetId(object entity, int id);
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Mapping;

namespace WeenyMapper.Reflection
{
    public interface IConventionReader : IConvention
    {
        IDictionary<string, object> GetColumnValues<T>(IDictionary<PropertyInfo, object> propertyValueMap);
        string GetTableName<T>();
        string GetPrimaryKeyColumnName<T>();
        string GetPrimaryKeyColumnName(Type type);
        object GetPrimaryKeyValue(object instance);
        IEnumerable<string> GetSelectableMappedPropertyNames(Type type);
        IDictionary<string, object> GetColumnValuesForInsertOrUpdate(object instance);
        string GetColumnName<T>(string propertyName);
        string GetColumnName(string propertyName, Type type);
        void SetId(object entity, int id);
        IEnumerable<string> GetSelectableColumNames(Type type);
        PropertyInfo GetPropertyForColumn(string columnName, Type type);
        bool IsForeignKey(string columnName, Type type);
        bool HasProperty(string columnName, Type type);
        bool IsEntityReferenceProperty(string columnName, Type type);
        PropertyInfo TryGetIdProperty(Type type);
        bool HasIdProperty(Type type);
        string TryGetPrimaryKeyColumnName(Type type);
        IDictionary<string, object> GetColumnValues(object instance);
    }
}
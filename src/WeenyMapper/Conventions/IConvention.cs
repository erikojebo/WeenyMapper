using System;
using System.Reflection;

namespace WeenyMapper.Conventions
{
    public interface IConvention
    {
        string GetColumnName(string propertyName);
        string GetTableName(string className);
        bool IsIdProperty(string propertyName);
        bool ShouldMapProperty(PropertyInfo propertyInfo);
        bool HasIdentityId(Type entityType);
    }
}
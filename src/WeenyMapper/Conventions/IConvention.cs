using System;
using System.Reflection;

namespace WeenyMapper.Conventions
{
    public interface IConvention
    {
        string GetColumnName(string propertyName);
        string GetTableName(Type entityType);
        bool IsIdProperty(PropertyInfo propertyInfo);
        bool ShouldMapProperty(PropertyInfo propertyInfo);
        bool HasIdentityId(Type entityType);
    }
}
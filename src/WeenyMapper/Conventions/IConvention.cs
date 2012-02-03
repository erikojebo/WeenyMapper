using System;
using System.Reflection;

namespace WeenyMapper.Conventions
{
    public interface IConvention
    {
        string GetColumnName(PropertyInfo propertyInfo);
        string GetTableName(Type entityType);
        string GetManyToOneForeignKeyColumnName(PropertyInfo propertyInfo);
        bool IsIdProperty(PropertyInfo propertyInfo);
        bool ShouldMapProperty(PropertyInfo propertyInfo);
        bool HasIdentityId(Type entityType);
        bool IsForeignKeyProperty(PropertyInfo propertyInfo);
    }
}
using System;
using System.Reflection;
using WeenyMapper.Reflection;

namespace WeenyMapper.Conventions
{
    public class DefaultConvention : IConvention
    {
        public virtual string GetColumnName(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name;
        }

        public virtual string GetTableName(Type entityType)
        {
            return entityType.Name;
        }

        public virtual bool IsIdProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name == "Id";
        }

        public virtual bool ShouldMapProperty(PropertyInfo propertyInfo)
        {
            const bool findNonPublic = true;

            var getter = propertyInfo.GetGetMethod(findNonPublic);
            var setter = propertyInfo.GetSetMethod(findNonPublic);

            return getter != null && getter.IsPublic &&
                   setter != null && setter.IsPublic &&
                   !getter.IsStatic && !setter.IsStatic;
        }

        public virtual bool HasIdentityId(Type entityType)
        {
            var dataReader = new ConventionReader(this);
            var idProperty = dataReader.GetIdProperty(entityType);

            return idProperty.PropertyType == typeof(int);
        }

        public virtual string GetManyToOneForeignKeyColumnName(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name + "Id";
        }

        public virtual bool IsForeignKeyProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name.EndsWith("Id") && !IsIdProperty(propertyInfo);
        }
    }
}
using System;
using System.Reflection;
using WeenyMapper.Reflection;

namespace WeenyMapper.Conventions
{
    public class DefaultConvention : IConvention
    {
        public virtual string GetColumnName(string propertyName)
        {
            return propertyName;
        }

        public virtual string GetTableName(string className)
        {
            return className;
        }

        public virtual bool IsIdProperty(string propertyName)
        {
            return propertyName == "Id";
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

        public bool HasIdentityId(Type entityType)
        {
            var dataReader = new ConventionDataReader(this);
            var idProperty = dataReader.GetIdProperty(entityType);

            return idProperty.PropertyType == typeof(int);
        }
    }
}
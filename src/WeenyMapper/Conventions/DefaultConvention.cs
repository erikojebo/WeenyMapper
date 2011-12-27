using System;

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
    }
}
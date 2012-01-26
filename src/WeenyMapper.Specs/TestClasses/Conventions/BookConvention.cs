using System;
using System.Reflection;
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class BookConvention : DefaultConvention
    {
        public override string GetColumnName(PropertyInfo propertyInfo)
        {
            return "c_" + propertyInfo.Name.ToUpper();
        }

        public override string GetTableName(Type entityType)
        {
            return "t_" + entityType.Name + "s";
        }

        public override bool IsIdProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name == "Isbn";
        }
    }
}
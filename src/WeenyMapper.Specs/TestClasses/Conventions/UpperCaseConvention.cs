using System;
using System.Reflection;
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class UpperCaseConvention : DefaultConvention
    {
        public override string GetColumnName(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name.ToUpper();
        }

        public override string GetTableName(Type entityType)
        {
            return entityType.Name.ToUpper();
        }
    }
}
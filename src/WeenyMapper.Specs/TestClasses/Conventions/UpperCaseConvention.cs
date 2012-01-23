using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class UpperCaseConvention : DefaultConvention
    {
        public override string GetColumnName(string propertyName)
        {
            return propertyName.ToUpper();
        }

        public override string GetTableName(Type entityType)
        {
            return entityType.Name.ToUpper();
        }
    }
}
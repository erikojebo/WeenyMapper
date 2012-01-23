using System;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class UserConvention : DefaultConvention
    {
        public override string GetTableName(Type entityType)
        {
            if (entityType.Name == "PartialUser")
            {
                return base.GetTableName(typeof(User));
            }

            return base.GetTableName(entityType);
        }
    }
}
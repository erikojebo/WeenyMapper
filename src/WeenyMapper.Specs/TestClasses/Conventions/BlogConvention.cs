using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class BlogConvention : DefaultConvention
    {
        public override string GetTableName(Type entityType)
        {
            if (entityType.Name == "BlogPost")
            {
                return "Post";
            }

            return base.GetTableName(entityType);
        }
    }
}
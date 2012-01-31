using System;
using System.Reflection;
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

        public override string GetManyToOneForeignKeyColumnName(PropertyInfo propertyInfo)
        {
            if (propertyInfo.Name == "BlogPost")
            {
                return "PostId";
            }

            return base.GetManyToOneForeignKeyColumnName(propertyInfo);
        }
    }
}
using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.Docs.Samples
{
    public class Convention : DefaultConvention
    {
        public override string GetTableName(Type entityType)
        {
            if (entityType.Name == "MiniBook")
                return "Book";

            return base.GetTableName(entityType);
        }
    }
}
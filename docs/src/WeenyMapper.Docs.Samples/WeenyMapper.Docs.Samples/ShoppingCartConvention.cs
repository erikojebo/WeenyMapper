using System;
using System.Reflection;
using WeenyMapper.Conventions;

namespace WeenyMapper.Docs.Samples
{
    public class ShoppingCartConvention : DefaultConvention
    {
        public override bool IsIdProperty(PropertyInfo propertyInfo)
        {
            // CartId is the primary key of the shopping cart table, but it does
            // not conform to our convention. So we need to special case it.
            if (propertyInfo.Name == "CartId")
                return true;
            
            // Product => ProductId
            return propertyInfo.Name == propertyInfo.DeclaringType.Name + "Id";
        }

        public override bool HasIdentityId(Type entityType)
        {
            return true;
        }

        public override string GetColumnName(PropertyInfo propertyInfo)
        {
            // For example C_PRICE
            return "C_" + propertyInfo.Name.ToUpper();
        }

        public override string GetTableName(Type entityType)
        {
            // For example T_PRODUCTS
            return "T_" + entityType.Name + "s";
        }

        public override bool ShouldMapProperty(PropertyInfo propertyInfo)
        {
            // Do not try to read/write any properties called IsDirty since they do not
            // exist in the database
            if (propertyInfo.Name == "IsDirty")
                return false;
            
            return base.ShouldMapProperty(propertyInfo);
        }
    }
}
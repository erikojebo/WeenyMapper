using System;
using System.Linq.Expressions;
using System.Reflection;
using WeenyMapper.Reflection;

namespace WeenyMapper.Mapping
{
    public class ObjectRelation
    {
        public ObjectRelation(PropertyInfo parentProperty, PropertyInfo childProperty, Type primaryType)
        {
            ParentProperty = parentProperty;
            ChildProperty = childProperty;
            PrimaryType = primaryType;
        }

        public static ObjectRelation Create<TParent, TChild>(
            Expression<Func<TParent, object>> parentProperty, 
            Expression<Func<TChild, TParent>> childProperty,
            Type primaryType)
        {
            return new ObjectRelation(
                Reflector<TParent>.GetProperty(parentProperty), 
                Reflector<TChild>.GetProperty(childProperty),
                primaryType);
        }

        public PropertyInfo ParentProperty { get; private set; }
        public PropertyInfo ChildProperty { get; private set; }
        public Type PrimaryType { get; private set; }
    }
}
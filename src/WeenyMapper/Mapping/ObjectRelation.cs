using System;
using System.Linq.Expressions;
using System.Reflection;
using WeenyMapper.Reflection;

namespace WeenyMapper.Mapping
{
    public class ObjectRelation
    {
        public static ObjectRelation Create<TParent, TChild>(Expression<Func<TParent, object>> parentProperty, Expression<Func<TChild, TParent>> childProperty)
        {
            return new ObjectRelation
                {
                    ParentProperty = Reflector<TParent>.GetProperty(parentProperty),
                    ChildProperty = Reflector<TChild>.GetProperty(childProperty)
                };
        }

        public PropertyInfo ParentProperty { get; private set; }
        public PropertyInfo ChildProperty { get; private set; }
    }
}
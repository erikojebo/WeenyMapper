using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.Mapping
{
    public class ObjectRelation
    {
        private ObjectRelation()
        {
        }

        private ObjectRelation(PropertyInfo parentProperty, PropertyInfo childProperty, Type primaryType)
        {
            ParentProperty = parentProperty;
            ChildProperty = childProperty;
            PrimaryType = primaryType;
            ChildType = childProperty.DeclaringType;
            ParentType = parentProperty.DeclaringType;
        }

        public static ObjectRelation CreateTwoWay<TParent, TChild>(
            Expression<Func<TParent, object>> parentProperty,
            Expression<Func<TChild, TParent>> childProperty,
            Type primaryType)
        {
            return new ObjectRelation(
                Reflector<TParent>.GetProperty(parentProperty),
                Reflector<TChild>.GetProperty(childProperty),
                primaryType);
        }

        public static ObjectRelation CreateParentToChild<TParent, TChild>(
            Expression<Func<TParent, IList<TChild>>> parentProperty)
        {
            return new ObjectRelation
                {
                    ParentProperty = Reflector<TParent>.GetProperty(parentProperty),
                    ChildType = typeof(TChild),
                    ParentType = typeof(TParent),
                    PrimaryType = typeof(TParent)
                };
        }

        public static ObjectRelation CreateChildToParent<TParent, TChild>(
            Expression<Func<TChild, TParent>> childProperty)
        {
            return new ObjectRelation
                {
                    ChildProperty = Reflector<TParent>.GetProperty(childProperty),
                    ChildType = typeof(TChild),
                    ParentType = typeof(TParent),
                    PrimaryType = typeof(TChild)
                };
        }

        public PropertyInfo ParentProperty { get; private set; }
        public PropertyInfo ChildProperty { get; private set; }
        public Type PrimaryType { get; private set; }
        public Type ChildType { get; private set; }
        public Type ParentType { get; private set; }

        public bool HasParentProperty
        {
            get { return ParentProperty != null; }
        }

        public bool HasChildProperty
        {
            get { return ChildProperty == null; }
        }

        public static ObjectRelation Create(ObjectQueryJoinSpecification joinSpecification, Type primaryType)
        {
            return new ObjectRelation
                {
                    ChildProperty = joinSpecification.ChildProperty,
                    ParentProperty = joinSpecification.ParentProperty,
                    ChildType = joinSpecification.ChildType,
                    ParentType = joinSpecification.ParentType,
                    PrimaryType = primaryType
                };
        }
    }
}
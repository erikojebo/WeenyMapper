using System;
using System.Reflection;

namespace WeenyMapper.Sql
{
    public class ObjectQueryJoinSpecification
    {
        private ObjectQueryJoinSpecification()
        {
        }

        public static ObjectQueryJoinSpecification CreateTwoWay(PropertyInfo parentProperty, PropertyInfo childProperty)
        {
            return new ObjectQueryJoinSpecification
                {
                    ParentProperty = parentProperty,
                    ChildProperty = childProperty,
                    ChildType = childProperty.DeclaringType,
                    ParentType = parentProperty.DeclaringType,
                };
        }

        public static ObjectQueryJoinSpecification CreateParentToChild(PropertyInfo parentProperty, Type childType)
        {
            return new ObjectQueryJoinSpecification
                {
                    ParentProperty = parentProperty,
                    ChildType = childType,
                    ParentType = parentProperty.DeclaringType,
                    ObjectQuerySpecification = new ObjectQuerySpecification(childType)
                };
        }

        public static ObjectQueryJoinSpecification CreateChildToParent(PropertyInfo childProperty, Type parentType)
        {
            return new ObjectQueryJoinSpecification
                {
                    ChildProperty = childProperty,
                    ChildType = childProperty.DeclaringType,
                    ParentType = parentType,
                    ObjectQuerySpecification = new ObjectQuerySpecification(parentType)
                };
        }

        public ObjectQuerySpecification ObjectQuerySpecification { get; set; }
        public PropertyInfo ParentProperty { get; private set; }
        public PropertyInfo ChildProperty { get; private set; }
        public Type ChildType { get; private set; }
        public Type ParentType { get; private set; }

        public bool HasChildProperty
        {
            get { return ChildProperty != null; }
        }

        public bool HasParentProperty
        {
            get { return ParentProperty != null; }
        }
    }
}
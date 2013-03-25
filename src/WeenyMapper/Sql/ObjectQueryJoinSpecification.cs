using System;
using System.Reflection;

namespace WeenyMapper.Sql
{
    public class ObjectQueryJoinSpecification
    {
        public ObjectQueryJoinSpecification(PropertyInfo parentProperty, PropertyInfo childProperty)
        {
            ParentProperty = parentProperty;
            ChildProperty = childProperty;

            ChildType = childProperty.DeclaringType;
            ParentType = parentProperty.DeclaringType;
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


    }
}
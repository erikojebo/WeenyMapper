using System;
using System.Reflection;

namespace WeenyMapper.Sql
{
    public class ObjectSubQueryJoin
    {
        private ObjectSubQueryJoin()
        {
        }

        public static ObjectSubQueryJoin CreateTwoWay(PropertyInfo parentProperty, PropertyInfo childProperty)
        {
            return new ObjectSubQueryJoin
                {
                    ParentProperty = parentProperty,
                    ChildProperty = childProperty,
                    ChildType = childProperty.DeclaringType,
                    ParentType = parentProperty.DeclaringType,
                };
        }

        public static ObjectSubQueryJoin CreateParentToChild(PropertyInfo parentProperty, PropertyInfo foreignKeyProperty)
        {
            return new ObjectSubQueryJoin
                {
                    ParentProperty = parentProperty,
                    ChildToParentForeignKeyProperty = foreignKeyProperty,
                    ChildType = foreignKeyProperty.DeclaringType,
                    ParentType = parentProperty.DeclaringType,
                    AliasedObjectSubQuery = new AliasedObjectSubQuery(foreignKeyProperty.DeclaringType)
                };
        }

        public static ObjectSubQueryJoin CreateChildToParent(PropertyInfo childProperty, Type parentType)
        {
            return new ObjectSubQueryJoin
                {
                    ChildProperty = childProperty,
                    ChildType = childProperty.DeclaringType,
                    ParentType = parentType,
                    AliasedObjectSubQuery = new AliasedObjectSubQuery(parentType)
                };
        }

        public AliasedObjectSubQuery AliasedObjectSubQuery { get; set; }
        public PropertyInfo ParentProperty { get; private set; }
        public PropertyInfo ChildProperty { get; private set; }
        public PropertyInfo ChildToParentForeignKeyProperty { get; private set; }
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

        public AliasedObjectSubQuery ChildSubQuery { get; set; }
        public AliasedObjectSubQuery ParentSubQuery { get; set; }
    }
}
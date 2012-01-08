using System;

namespace WeenyMapper.QueryParsing
{
    public class PropertyExpression : EquatableQueryExpression<PropertyExpression>
    {
        public PropertyExpression(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; private set; }

        public override int GetHashCode()
        {
            return PropertyName.GetHashCode();
        }

        protected override bool NullSafeEquals(PropertyExpression other)
        {
             return PropertyName == other.PropertyName;
        }

        public override string ToString()
        {
            return "[" + PropertyName + "]";
        }
    }
}
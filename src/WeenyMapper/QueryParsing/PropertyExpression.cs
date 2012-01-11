using System;
using WeenyMapper.Conventions;

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

        public override QueryExpression Translate(IConvention convention)
        {
            var columnName = convention.GetColumnName(PropertyName);
            return new PropertyExpression(columnName);
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
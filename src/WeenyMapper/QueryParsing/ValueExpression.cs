using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public class ValueExpression : EquatableQueryExpression<ValueExpression>
    {
        public ValueExpression(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        protected override bool NullSafeEquals(ValueExpression other)
        {
            return Value.Equals(other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
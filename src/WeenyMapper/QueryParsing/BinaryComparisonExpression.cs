using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public abstract class BinaryComparisonExpression<T> : EquatableQueryExpression<BinaryComparisonExpression<T>> where T : BinaryComparisonExpression<T>
    {
        protected BinaryComparisonExpression(PropertyExpression propertyExpression, ValueExpression valueExpression)
        {
            PropertyExpression = propertyExpression;
            ValueExpression = valueExpression;
        }

        public PropertyExpression PropertyExpression { get; private set; }
        public ValueExpression ValueExpression { get; private set; }

        public abstract string OperatorString { get; }

        public override int GetHashCode()
        {
            return PropertyExpression.GetHashCode() + ValueExpression.GetHashCode();
        }

        protected override bool NullSafeEquals(BinaryComparisonExpression<T> other)
        {
            return PropertyExpression.Equals(other.PropertyExpression) && ValueExpression.Equals(other.ValueExpression);
        }

        public override string ToString()
        {
            return string.Format("({0} {2} {1})", PropertyExpression, ValueExpression, OperatorString);
        }

        public override QueryExpression Translate(IConvention convention)
        {
            return Create(
                (PropertyExpression)PropertyExpression.Translate(convention),
                (ValueExpression)ValueExpression.Translate(convention));
        }

        protected abstract QueryExpression Create(PropertyExpression propertyExpression, ValueExpression valueExpression);
    }
}
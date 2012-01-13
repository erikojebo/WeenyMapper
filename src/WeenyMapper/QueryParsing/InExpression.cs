namespace WeenyMapper.QueryParsing
{
    public class InExpression : EquatableQueryExpression<InExpression>
    {
        public InExpression(PropertyExpression propertyExpression, ArrayValueExpression arrayValueExpression)
        {
            PropertyExpression = propertyExpression;
            ArrayValueExpression = arrayValueExpression;
        }

        public PropertyExpression PropertyExpression { get; private set; }
        public ArrayValueExpression ArrayValueExpression { get; private set; }

        public override int GetHashCode()
        {
            return PropertyExpression.GetHashCode() + ArrayValueExpression.GetHashCode();
        }

        protected override bool NullSafeEquals(InExpression other)
        {
            return Equals(PropertyExpression, other.PropertyExpression) &&
                   Equals(ArrayValueExpression, other.ArrayValueExpression);
        }

        public override string ToString()
        {
            return string.Format("{0} in {1}", PropertyExpression, ArrayValueExpression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
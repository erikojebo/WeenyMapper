namespace WeenyMapper.QueryParsing
{
    public class GreaterOrEqualExpression : BinaryComparisonExpression<GreaterOrEqualExpression>
    {
        public GreaterOrEqualExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return ">="; }
        }
    }
}
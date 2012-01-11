namespace WeenyMapper.QueryParsing
{
    public class LessOrEqualExpression : BinaryComparisonExpression<LessOrEqualExpression>
    {
        public LessOrEqualExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return "<="; }
        }
    }
}
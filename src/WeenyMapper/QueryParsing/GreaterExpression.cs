namespace WeenyMapper.QueryParsing
{
    public class GreaterExpression : BinaryComparisonExpression<GreaterExpression>
    {
        public GreaterExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return ">"; }
        }
    }
}
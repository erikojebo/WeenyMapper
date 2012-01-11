namespace WeenyMapper.QueryParsing
{
    public class LessExpression : BinaryComparisonExpression<LessExpression>
    {
        public LessExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return "<"; }
        }
    }
}
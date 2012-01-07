namespace WeenyMapper.QueryParsing
{
    public class GreaterOrEqualExpression : BinaryComparisonExpression<GreaterOrEqualExpression>
    {
        public GreaterOrEqualExpression(QueryExpression left, QueryExpression right) : base(left, right) {}

        protected override string OperatorString
        {
            get { return ">="; }
        }
    }
}
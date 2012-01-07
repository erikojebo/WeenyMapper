namespace WeenyMapper.QueryParsing
{
    public class LessOrEqualExpression : BinaryComparisonExpression<LessOrEqualExpression>
    {
        public LessOrEqualExpression(QueryExpression left, QueryExpression right) : base(left, right) {}

        protected override string OperatorString
        {
            get { return "<="; }
        }
    }
}
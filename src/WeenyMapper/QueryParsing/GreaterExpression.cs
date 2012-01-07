namespace WeenyMapper.QueryParsing
{
    public class GreaterExpression : BinaryComparisonExpression<GreaterExpression>
    {
        public GreaterExpression(QueryExpression left, QueryExpression right) : base(left, right) {}

        protected override string OperatorString
        {
            get { return ">"; }
        }
    }
}
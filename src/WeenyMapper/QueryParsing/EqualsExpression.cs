namespace WeenyMapper.QueryParsing
{
    public class EqualsExpression : BinaryComparisonExpression<EqualsExpression>
    {
        public EqualsExpression(QueryExpression left, QueryExpression right) : base(left, right) {}

        protected override string OperatorString
        {
            get { return "=="; }
        }
    }
}
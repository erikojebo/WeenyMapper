namespace WeenyMapper.QueryParsing
{
    public class LessExpression : BinaryComparisonExpression<LessExpression>
    {
        public LessExpression(QueryExpression left, QueryExpression right) : base(left, right) {}

        protected override string OperatorString
        {
            get { return "<"; }
        }
    }
}
namespace WeenyMapper.QueryParsing
{
    public class AndExpression : PolyadicOperatorExpression<AndExpression>
    {
        public AndExpression(params QueryExpression[] expressions) : base(expressions) {}

        protected override AndExpression Create(params QueryExpression[] expressions)
        {
            return new AndExpression(expressions);
        }

        protected override string OperatorString
        {
            get { return "&&"; }
        }
    }
}
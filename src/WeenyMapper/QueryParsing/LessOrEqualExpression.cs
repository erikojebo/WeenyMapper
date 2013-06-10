namespace WeenyMapper.QueryParsing
{
    public class LessOrEqualExpression : BinaryComparisonExpression<LessOrEqualExpression>
    {
        public LessOrEqualExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) {}

        protected override string OperatorString
        {
            get { return "<="; }
        }

        protected override QueryExpression Create(PropertyExpression propertyExpression, ValueExpression valueExpression)
        {
            return new LessOrEqualExpression(propertyExpression, valueExpression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
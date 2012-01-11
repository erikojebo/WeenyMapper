namespace WeenyMapper.QueryParsing
{
    public class EqualsExpression : BinaryComparisonExpression<EqualsExpression>
    {
        public EqualsExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return "=="; }
        }

        public override void Visit(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Accept(this);
        }
    }
}
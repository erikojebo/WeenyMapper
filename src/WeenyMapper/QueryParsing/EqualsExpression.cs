namespace WeenyMapper.QueryParsing
{
    public class EqualsExpression : BinaryComparisonExpression<EqualsExpression>
    {
        public EqualsExpression(string propertyName, object value) : this(new PropertyExpression(propertyName), new ValueExpression(value)) {}

        public EqualsExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) {}

        protected override string OperatorString
        {
            get { return "=="; }
        }

        protected override QueryExpression Create(PropertyExpression propertyExpression, ValueExpression valueExpression)
        {
            return new EqualsExpression(propertyExpression, valueExpression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
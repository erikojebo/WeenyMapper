namespace WeenyMapper.QueryParsing
{
    public class NotEqualExpression : BinaryComparisonExpression<NotEqualExpression>
    {
        public NotEqualExpression(string propertyName, object value) : this(new PropertyExpression(propertyName), new ValueExpression(value)) {}

        public NotEqualExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) {}

        protected override string OperatorString
        {
            get { return "!="; }
        }

        protected override QueryExpression Create(PropertyExpression propertyExpression, ValueExpression valueExpression)
        {
            return new NotEqualExpression(propertyExpression, valueExpression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
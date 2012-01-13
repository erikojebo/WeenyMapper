using System;

namespace WeenyMapper.QueryParsing
{
    public class GreaterOrEqualExpression : BinaryComparisonExpression<GreaterOrEqualExpression>
    {
        public GreaterOrEqualExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return ">="; }
        }

        protected override QueryExpression Create(PropertyExpression propertyExpression, ValueExpression valueExpression)
        {
            return new GreaterOrEqualExpression(propertyExpression, valueExpression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
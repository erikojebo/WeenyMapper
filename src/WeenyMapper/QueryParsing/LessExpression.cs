using System;

namespace WeenyMapper.QueryParsing
{
    public class LessExpression : BinaryComparisonExpression<LessExpression>
    {
        public LessExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return "<"; }
        }

        protected override QueryExpression Create(PropertyExpression propertyExpression, ValueExpression valueExpression)
        {
            return new LessExpression(propertyExpression, valueExpression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
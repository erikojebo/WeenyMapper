using System;

namespace WeenyMapper.QueryParsing
{
    public class GreaterExpression : BinaryComparisonExpression<GreaterExpression>
    {
        public GreaterExpression(PropertyExpression propertyExpression, ValueExpression valueExpression) : base(propertyExpression, valueExpression) { }

        protected override string OperatorString
        {
            get { return ">"; }
        }
        
        protected override QueryExpression Create(PropertyExpression propertyExpression, ValueExpression valueExpression)
        {
            return new GreaterExpression(propertyExpression, valueExpression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
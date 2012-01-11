using System;
using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.QueryParsing
{
    public class OrExpression : PolyadicOperatorExpression<OrExpression>
    {
        public OrExpression(params QueryExpression[] expressions) : base(expressions) {}

        protected override OrExpression Create(params QueryExpression[] expressions)
        {
            return new OrExpression(expressions);
        }

        protected override string OperatorString
        {
            get { return " || "; }
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
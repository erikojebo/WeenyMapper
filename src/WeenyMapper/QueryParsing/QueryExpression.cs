using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public abstract class QueryExpression
    {
        public virtual QueryExpression Translate(IConvention convention)
        {
            return this;
        }

        public abstract void Accept(IExpressionVisitor expressionVisitor);

        public static RootExpression Create(QueryExpression queryExpression)
        {
            return new RootExpression(queryExpression);
        }
        
        public static RootExpression Create()
        {
            return new RootExpression();
        }
    }
}
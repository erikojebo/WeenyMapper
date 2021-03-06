﻿namespace WeenyMapper.QueryParsing
{
    public interface IExpressionVisitor
    {
        void Visit(AndExpression expression);
        void Visit(OrExpression expression);
        void Visit(ValueExpression expression);
        void Visit(PropertyExpression expression);
        void Visit(InExpression expression);
        void Visit(EqualsExpression expression);
        void Visit(LessOrEqualExpression expression);
        void Visit(LessExpression expression);
        void Visit(GreaterOrEqualExpression expression);
        void Visit(GreaterExpression expression);
        void Visit(RootExpression expression);
        void Visit(LikeExpression expression);
        void Visit(EntityReferenceExpression expression);
        void Visit(NotEqualExpression expression);
        void Visit(NotExpression expression);
    }
}
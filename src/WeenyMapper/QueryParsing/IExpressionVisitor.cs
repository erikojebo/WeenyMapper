namespace WeenyMapper.QueryParsing
{
    public interface IExpressionVisitor
    {
        void Visit(EqualsExpression expression);
        void Visit(AndExpression expression);
        void Visit(OrExpression expression);
    }
}
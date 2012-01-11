namespace WeenyMapper.QueryParsing
{
    public interface IExpressionVisitor {
        void Accept(EqualsExpression equalsExpression);
    }
}
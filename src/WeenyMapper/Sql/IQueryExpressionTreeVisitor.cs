namespace WeenyMapper.Sql
{
    public interface IQueryExpressionTreeVisitor
    {
        void Visit(QueryExpressionTreeAndBranch tree);
        void Visit(QueryExpressionTreeOrBranch tree);
        void Visit(QueryExpressionTreeLeaf tree);
        void Visit(EmptyQueryExpressionTree tree);
    }
}
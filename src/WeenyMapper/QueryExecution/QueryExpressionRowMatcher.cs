using WeenyMapper.Mapping;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class QueryExpressionRowMatcher : IQueryExpressionTreeVisitor
    {
        private readonly Row _row;
        private readonly IConventionReader _conventionReader;
        private bool _isMatch;

        public QueryExpressionRowMatcher(Row row, IConventionReader conventionReader)
        {
            _row = row;
            _conventionReader = conventionReader;
        }

        public void Visit(QueryExpressionTreeAndBranch tree)
        {
            foreach (var node in tree.Nodes)
            {
                var matcher = new QueryExpressionRowMatcher(_row, _conventionReader);
                if (!matcher.Matches(node))
                {
                    _isMatch = false;
                    return;
                }
            }
        }

        public void Visit(QueryExpressionTreeOrBranch tree)
        {
            foreach (var node in tree.Nodes)
            {
                var matcher = new QueryExpressionRowMatcher(_row, _conventionReader);
                if (matcher.Matches(node))
                {
                    return;
                }
            }

            _isMatch = false;
        }

        public void Visit(QueryExpressionTreeLeaf tree)
        {
            var matcher = new InMemoryRowMatcher(_row, tree.QueryExpression.Translate(_conventionReader));

            if (!matcher.IsMatch())
                _isMatch = false;
        }

        public void Visit(EmptyQueryExpressionTree tree)
        {
        }

        public bool Matches(QueryExpressionTree expressionTree)
        {
            _isMatch = true;

            expressionTree.Accept(this);

            return _isMatch;
        }
    }
}
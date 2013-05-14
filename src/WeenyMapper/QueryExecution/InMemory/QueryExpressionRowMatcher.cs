using WeenyMapper.Exceptions;
using WeenyMapper.Mapping;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution.InMemory
{
    public class QueryExpressionRowMatcher : IQueryExpressionTreeVisitor
    {
        private readonly Row _row;
        private bool _isMatch;

        public QueryExpressionRowMatcher(Row row)
        {
            _row = row;
        }

        public void Visit(QueryExpressionTreeAndBranch tree)
        {
            foreach (var node in tree.Nodes)
            {
                var matcher = new QueryExpressionRowMatcher(_row);
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
                var matcher = new QueryExpressionRowMatcher(_row);
                if (matcher.Matches(node))
                {
                    return;
                }
            }

            _isMatch = false;
        }

        public void Visit(QueryExpressionTreeLeaf tree)
        {
            throw new WeenyMapperException("Expression tree must be translated before being used to filter the result set");
        }

        public void Visit(EmptyQueryExpressionTree tree)
        {
        }

        public void Visit(TranslatedQueryExpressionTreeLeaf tree)
        {
            var matcher = new InMemoryRowMatcher(_row, tree.QueryExpression, tree.TableIdentifier);

            if (!matcher.IsMatch())
                _isMatch = false;
        }

        public bool Matches(QueryExpressionTree expressionTree)
        {
            _isMatch = true;

            expressionTree.Accept(this);

            return _isMatch;
        }
    }
}
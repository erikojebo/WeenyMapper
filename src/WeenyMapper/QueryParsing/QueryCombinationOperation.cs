using WeenyMapper.Extensions;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryParsing
{
    public class QueryCombinationOperation
    {
        private readonly string _operatorString;
        private readonly QueryOptimizer _queryOptimizer = new QueryOptimizer();

        public static readonly QueryCombinationOperation And = new QueryCombinationOperation("AND");
        public static readonly QueryCombinationOperation Or = new QueryCombinationOperation("OR");

        private QueryCombinationOperation(string operatorString)
        {
            _operatorString = operatorString;
        }

        public string Combine(string leftConstraint, string rightConstraint)
        {
            if (leftConstraint.IsNullOrWhiteSpace())
                return rightConstraint;
            if (rightConstraint.IsNullOrWhiteSpace())
                return leftConstraint;

            if (ContainsOperatorString(Or, leftConstraint) && !_queryOptimizer.CanOutermostParensBeReduced(leftConstraint))
                leftConstraint = string.Format("({0})", leftConstraint);
            
            return string.Format("{0} {1} {2}", leftConstraint, _operatorString, rightConstraint);
        }

        private static bool ContainsOperatorString(QueryCombinationOperation queryCombinationOperation, string leftConstraint)
        {
            return leftConstraint.Contains(string.Format(" {0} ", queryCombinationOperation._operatorString));
        }
    }
}
using WeenyMapper.Extensions;

namespace WeenyMapper.QueryParsing
{
    public class QueryCombinationOperation
    {
        private readonly string _operatorString;

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

            return string.Format("{0} {1} {2}", leftConstraint, _operatorString, rightConstraint);
        }
    }
}
namespace WeenyMapper.QueryParsing
{
    public class AndExpression : QueryExpression
    {
        public AndExpression(QueryExpression left, QueryExpression right)
        {
            Left = left;
            Right = right;
        }

        public QueryExpression Left { get; private set; }
        public QueryExpression Right { get; private set; }

        public override int GetHashCode()
        {
            return Left.GetHashCode() + Right.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as AndExpression;

            if (other == null)
            {
                return false;
            }

            return Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        public override string ToString()
        {
            return "(" + Left + " && " + Right + ")";
        }
    }
}
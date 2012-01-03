namespace WeenyMapper.QueryParsing
{
    public class EqualsExpression : QueryExpression
    {
        public EqualsExpression(QueryExpression left, QueryExpression right)
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
            var other = obj as EqualsExpression;

            if (other == null)
            {
                return false;
            }

            return Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        public override string ToString()
        {
            return string.Format("({0} == {1})", Left, Right);
        }
    }
}
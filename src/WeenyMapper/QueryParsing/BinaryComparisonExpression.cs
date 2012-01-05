namespace WeenyMapper.QueryParsing
{
    public abstract class BinaryComparisonExpression<T> : QueryExpression where T : BinaryComparisonExpression<T>
    {
        protected BinaryComparisonExpression(QueryExpression left, QueryExpression right)
        {
            Left = left;
            Right = right;
        }

        public QueryExpression Left { get; private set; }
        public QueryExpression Right { get; private set; }
        
        protected abstract string OperatorString { get; }

        public override int GetHashCode()
        {
            return Left.GetHashCode() + Right.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as T;

            if (other == null)
            {
                return false;
            }

            return Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        public override string ToString()
        {
            return string.Format("({0} {2} {1})", Left, Right, OperatorString);
        }
    }
}
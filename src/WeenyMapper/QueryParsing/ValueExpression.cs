namespace WeenyMapper.QueryParsing
{
    public class ValueExpression : QueryExpression
    {
        public ValueExpression(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ValueExpression;

            if (other == null)
            {
                return false;
            }

            return Value.Equals(other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
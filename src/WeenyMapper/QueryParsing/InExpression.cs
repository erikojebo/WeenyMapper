namespace WeenyMapper.QueryParsing
{
    public class InExpression : QueryExpression
    {
        public InExpression(PropertyExpression propertyExpression, ArrayValueExpression arrayValueExpression)
        {
            PropertyExpression = propertyExpression;
            ArrayValueExpression = arrayValueExpression;
        }

        public PropertyExpression PropertyExpression { get; private set; }
        public ArrayValueExpression ArrayValueExpression { get; private set; }

        public override int GetHashCode()
        {
            return PropertyExpression.GetHashCode() + ArrayValueExpression.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as InExpression;

            if (other == null)
            {
                return false;
            }

            return Equals(PropertyExpression, other.PropertyExpression) &&
                   Equals(ArrayValueExpression, other.ArrayValueExpression);
        }

        public override string ToString()
        {
            return string.Format("{0} in {1}", PropertyExpression, ArrayValueExpression);
        }
    }
}
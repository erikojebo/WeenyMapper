namespace WeenyMapper.QueryParsing
{
    public class PropertyExpression : QueryExpression
    {
        public PropertyExpression(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; private set; }

        public override int GetHashCode()
        {
            return PropertyName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as PropertyExpression;

            if (other == null)
            {
                return false;
            }

            return PropertyName == other.PropertyName;
        }

        public override string ToString()
        {
            return "[" + PropertyName + "]";
        }
    }
}
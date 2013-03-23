using System;

namespace WeenyMapper.QueryParsing
{
    public class PropertyExpression : EquatableQueryExpression<PropertyExpression>
    {
        public PropertyExpression(string propertyName)
        {
            PropertyName = propertyName;
        }

        public PropertyExpression(string propertyName, Type propertyType) : this(propertyName)
        {
            PropertyType = propertyType;
        }

        public string PropertyName { get; private set; }
        public Type PropertyType { get; set; }

        public override int GetHashCode()
        {
            return PropertyName.GetHashCode();
        }

        protected override bool NullSafeEquals(PropertyExpression other)
        {
            return PropertyName == other.PropertyName &&
                PropertyType == other.PropertyType;
        }

        public override string ToString()
        {
            var propertyTypeName = PropertyType == null ? "NULL" : PropertyType.Name;
            return string.Format("[{0} ({1})]", PropertyName, propertyTypeName);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }
    }
}
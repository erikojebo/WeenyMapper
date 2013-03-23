using System.Reflection;
using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public class ReflectedPropertyExpression : PropertyExpression
    {
        private readonly PropertyInfo _propertyInfo;

        public ReflectedPropertyExpression(PropertyInfo propertyInfo) : base(propertyInfo.Name, propertyInfo.PropertyType)
        {
            _propertyInfo = propertyInfo;
        }

        public override QueryExpression Translate(IConvention convention)
        {
            var columnName = convention.GetColumnName(_propertyInfo);
            return new PropertyExpression(columnName, PropertyType);
        }
    }
}
using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public class LikeExpression : EquatableQueryExpression<LikeExpression>
    {
        public LikeExpression(PropertyExpression propertyExpression, string searchString)
        {
            PropertyExpression = propertyExpression;
            SearchString = searchString;
        }

        public PropertyExpression PropertyExpression { get; set; }
        public string SearchString { get; set; }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }

        public override int GetHashCode()
        {
            return PropertyExpression.GetHashCode();
        }

        protected override bool NullSafeEquals(LikeExpression other)
        {
            return Equals(PropertyExpression, other.PropertyExpression) &&
                   SearchString == other.SearchString;
        }

        public override QueryExpression Translate(IConvention convention)
        {
            return new LikeExpression((PropertyExpression)PropertyExpression.Translate(convention), SearchString);
        }
    }
}
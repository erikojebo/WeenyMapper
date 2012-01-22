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
        public bool HasStartingWildCard { get; set; }
        public bool HasEndingWildCard { get; set; }

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
                   SearchString == other.SearchString &&
                   HasStartingWildCard == other.HasStartingWildCard &&
                   HasEndingWildCard == other.HasEndingWildCard;
        }

        public override string ToString()
        {
            var searchString = SearchString;

            if (HasStartingWildCard)
            {
                searchString = "%" + searchString;
            }
            if (HasEndingWildCard)
            {
                searchString = searchString + "%";
            }

            return string.Format("({0} LIKE {1})", PropertyExpression.PropertyName, searchString);
        }

        public override QueryExpression Translate(IConvention convention)
        {
            return new LikeExpression((PropertyExpression)PropertyExpression.Translate(convention), SearchString)
                {
                    HasStartingWildCard = HasStartingWildCard,
                    HasEndingWildCard = HasEndingWildCard
                };
        }
    }
}
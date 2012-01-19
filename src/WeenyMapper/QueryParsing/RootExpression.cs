using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public class RootExpression : EquatableQueryExpression<RootExpression>
    {
        public RootExpression() {}

        public RootExpression(QueryExpression childExpression)
        {
            ChildExpression = childExpression;
        }

        public QueryExpression ChildExpression { get; set; }

        public bool HasChildExpression
        {
            get { return ChildExpression != null; }
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }

        public override QueryExpression Translate(IConvention convention)
        {
            if (HasChildExpression)
            {
                return new RootExpression(ChildExpression.Translate(convention));
            }

            return this;
        }

        public override int GetHashCode()
        {
            if (HasChildExpression)
            {
                return ChildExpression.GetHashCode();
            }

            return 0;
        }

        protected override bool NullSafeEquals(RootExpression other)
        {
            return Equals(ChildExpression, other.ChildExpression);
        }
    }
}
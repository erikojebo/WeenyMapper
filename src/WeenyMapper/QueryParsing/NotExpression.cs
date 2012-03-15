using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public class NotExpression : EquatableQueryExpression<NotExpression>
    {
        public NotExpression(QueryExpression expression)
        {
            Expression = expression;
        }

        public QueryExpression Expression { get; set; }

        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }

        protected override bool NullSafeEquals(NotExpression other)
        {
            return Expression.Equals(other.Expression);
        }

        public override string ToString()
        {
            return string.Format("!({0})", Expression);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }

        public override QueryExpression Translate(IConvention convention)
        {
            return new NotExpression(Expression.Translate(convention));
        }
    }
}
using System.Reflection;
using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public class EntityReferenceExpression : PropertyExpression
    {
        public EntityReferenceExpression(PropertyInfo referencePropertyInfo, PropertyInfo dataPropertyInfo) : base(dataPropertyInfo.DeclaringType.Name + "." + dataPropertyInfo.Name)
        {
            ReferencePropertyInfo = referencePropertyInfo;
            DataPropertyInfo = dataPropertyInfo;
        }

        public PropertyInfo ReferencePropertyInfo { get; private set; }
        public PropertyInfo DataPropertyInfo { get; private set; }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(this);
        }

        public override int GetHashCode()
        {
            return DataPropertyInfo.GetHashCode();
        }

        protected override bool NullSafeEquals(PropertyExpression other)
        {
            var otherReference = other as EntityReferenceExpression;

            if (otherReference == null)
            {
                return false;
            }

            return Equals(DataPropertyInfo, otherReference.DataPropertyInfo) &&
                Equals(ReferencePropertyInfo, otherReference.ReferencePropertyInfo);
        }

        public override QueryExpression Translate(IConvention convention)
        {
            var columnName = convention.GetManyToOneForeignKeyColumnName(ReferencePropertyInfo);

            return new PropertyExpression(columnName);
        }
    }
}
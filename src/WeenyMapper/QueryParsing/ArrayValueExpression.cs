using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Extensions;

namespace WeenyMapper.QueryParsing
{
    public class ArrayValueExpression : EquatableQueryExpression<ArrayValueExpression>
    {
        public ArrayValueExpression(IEnumerable values)
        {
            Values = values.OfType<object>().ToArray();
        }

        public object[] Values { get; set; }

        public override int GetHashCode()
        {
            return Values.GetHashCode();
        }

        protected override bool NullSafeEquals(ArrayValueExpression other)
        {
            return Values.ElementEquals(other.Values);
        }

        public override string ToString()
        {
            var values = string.Join(", ", Values);
            return string.Format("({0})", values);
        }

        public override void Accept(IExpressionVisitor expressionVisitor)
        {
            throw new NotImplementedException();
        }
    }
}
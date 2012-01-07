using System;
using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.QueryParsing
{
    public abstract class PolyadicOperatorExpression<T> : QueryExpression where T : PolyadicOperatorExpression<T>
    {
        protected PolyadicOperatorExpression(params QueryExpression[] expressions)
        {
            Expressions = expressions;
        }

        public IList<QueryExpression> Expressions { get; private set; }

        protected abstract T Create(params QueryExpression[] expressions);

        protected abstract string OperatorString { get; }

        public override int GetHashCode()
        {
            return Expressions.Sum(x => x.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            var other = obj as T;

            if (other == null || Expressions.Count != other.Expressions.Count)
            {
                return false;
            }

            for (int i = 0; i < Expressions.Count; i++)
            {
                if (!Expressions[i].Equals(other.Expressions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            var separator = String.Format(" {0} ", OperatorString);
            var expression = string.Join(separator, Expressions);
            return string.Format("({0})", expression);
        }

        public T Flatten()
        {
            var expressions = new List<QueryExpression>();

            foreach (var queryExpression in Expressions)
            {
                if (queryExpression is T)
                {
                    var andExpression = ((T)queryExpression).Flatten();
                    expressions.AddRange(andExpression.Expressions);
                }
                else
                {
                    expressions.Add(queryExpression);
                }
            }

            return Create(expressions.ToArray());
        }
    }
}
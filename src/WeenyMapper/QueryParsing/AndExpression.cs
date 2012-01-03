using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.QueryParsing
{
    public class AndExpression : QueryExpression
    {
        public AndExpression(params QueryExpression[] expressions)
        {
            Expressions = expressions;
        }

        public IList<QueryExpression> Expressions { get; private set; }

        public override int GetHashCode()
        {
            return Expressions.Sum(x => x.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            var other = obj as AndExpression;

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
            var conjunction = string.Join(" && ", Expressions);
            return string.Format("({0})", conjunction);
        }

        public AndExpression Flatten()
        {
            var expressions = new List<QueryExpression>();

            foreach (var queryExpression in Expressions)
            {
                if (queryExpression is AndExpression)
                {
                    var andExpression = ((AndExpression)queryExpression).Flatten();
                    expressions.AddRange(andExpression.Expressions);
                }
                else
                {
                    expressions.Add(queryExpression);
                }
            }

            return new AndExpression(expressions.ToArray());
        }
    }
}
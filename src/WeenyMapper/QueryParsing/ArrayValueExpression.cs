using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.QueryParsing
{
    public class ArrayValueExpression : QueryExpression
    {
        public ArrayValueExpression(IEnumerable<object> values)
        {
            Values = values.ToArray();
        }

        public object[] Values { get; set; }

        public override int GetHashCode()
        {
            return Values.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ArrayValueExpression;

            if (other == null || Values.Length != other.Values.Length)
            {
                return false;
            }

            for (int i = 0; i < Values.Length; i++)
            {
                if (!Equals(Values[i], other.Values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            var values = string.Join(", ", Values);
            return string.Format("({0})", values);
        }
    }
}
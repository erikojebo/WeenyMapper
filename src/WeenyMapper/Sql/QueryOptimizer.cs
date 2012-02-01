using System;

namespace WeenyMapper.Sql
{
    public class QueryOptimizer
    {
        public string ReduceParens(string expression)
        {
            if (expression.StartsWith("(") && expression.EndsWith(")"))
                return expression.Substring(1, expression.Length - 2);

            return expression;
        }
    }
}
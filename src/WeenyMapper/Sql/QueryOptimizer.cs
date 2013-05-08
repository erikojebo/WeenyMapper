using System.Collections.Generic;

namespace WeenyMapper.Sql
{
    public class QueryOptimizer
    {
        public string ReduceParens(string expression)
        {
            var reducedExpression = expression;

            while (CanOutermostParensBeReduced(reducedExpression))
            {
                reducedExpression = StripFirstAndLastChar(reducedExpression);
            }

            return reducedExpression;
        }

        public bool CanOutermostParensBeReduced(string expression)
        {
            return expression.Length >= 2 && 
                expression.StartsWith("(") && expression.EndsWith(")") && 
                IsParensBalanced(StripFirstAndLastChar(expression));
        }

        private string StripFirstAndLastChar(string expression)
        {
            return expression.Substring(1, expression.Length - 2);
        }

        private bool IsParensBalanced(IEnumerable<char> expressionWithStrippedOuterParens)
        {
            var nestingLevel = 0;

            foreach (var character in expressionWithStrippedOuterParens)
            {
                if (character == '(')
                {
                    nestingLevel++;
                }
                if (character == ')')
                {
                    nestingLevel--;
                }

                if (nestingLevel < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
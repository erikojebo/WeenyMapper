using System;
using System.Linq;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryRowMatcher : IExpressionVisitor
    {
        private readonly Row _row;
        private readonly QueryExpression _queryExpression;
        private bool _isMatch;

        public InMemoryRowMatcher(Row row, QueryExpression queryExpression)
        {
            _row = row;
            _queryExpression = queryExpression;
        }

        public void Visit(AndExpression expression)
        {
            foreach (var queryExpression in expression.Expressions)
            {
                var matcher = new InMemoryRowMatcher(_row, queryExpression);

                if (!matcher.IsMatch())
                {
                    _isMatch = false;
                    return;
                }
            }
        }

        public void Visit(OrExpression expression)
        {
            foreach (var queryExpression in expression.Expressions)
            {
                var matcher = new InMemoryRowMatcher(_row, queryExpression);

                if (matcher.IsMatch())
                {
                    return;
                }
            }

            _isMatch = false;
        }

        public void Visit(ValueExpression expression)
        {
        }

        public void Visit(PropertyExpression expression)
        {
            if (expression.PropertyType == typeof(bool))
            {
                var columnName = expression.PropertyName;
                MatchValue(columnName, true);
            }
        }

        public void Visit(InExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(EqualsExpression expression)
        {
            var columnName = expression.PropertyExpression.PropertyName;
            var value = expression.ValueExpression.Value;

            MatchValue(columnName, value);
        }

        private void MatchValue(string columnName, object value)
        {
            var isMatch = IsMatch(columnName, value);

            if (!isMatch)
                _isMatch = false;
        }

        private bool IsMatch(string columnName, object value)
        {
            var columnValue = _row.ColumnValues.First(x => x.ColumnName == columnName).Value;

            return Equals(value, columnValue);
        }

        public void Visit(LessOrEqualExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(LessExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(GreaterOrEqualExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(GreaterExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(RootExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(LikeExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(EntityReferenceExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(NotEqualExpression expression)
        {
            var isColumnValueMatch = IsMatch(expression.PropertyExpression.PropertyName, expression.ValueExpression.Value);

            if (isColumnValueMatch)
                _isMatch = false;
        }

        public void Visit(NotExpression expression)
        {
            var matcher = new InMemoryRowMatcher(_row, expression.Expression);

            if (matcher.IsMatch())
                _isMatch = false;
        }

        public bool IsMatch()
        {
            _isMatch = true;

            _queryExpression.Accept(this);

            return _isMatch;
        }
    }
}
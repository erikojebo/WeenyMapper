using System;
using System.Linq;
using WeenyMapper.Exceptions;
using WeenyMapper.Extensions;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution.InMemory
{
    public class InMemoryRowMatcher : IExpressionVisitor
    {
        private readonly Row _row;
        private readonly QueryExpression _queryExpression;
        private readonly string _tableIdentifier;
        private bool _isMatch;

        public InMemoryRowMatcher(Row row, QueryExpression queryExpression, string tableIdentifier = null)
        {
            _row = row;
            _queryExpression = queryExpression;
            _tableIdentifier = tableIdentifier;
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
                var matcher = new InMemoryRowMatcher(_row, queryExpression, _tableIdentifier);

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
            var values = expression.ArrayValueExpression.Values;

            if (values.IsEmpty())
            {
                throw new WeenyMapperException("Can not generate IN constraint from empty collection");
            }

            var columnName = expression.PropertyExpression.PropertyName;
            var actualValue = _row.GetColumnValue(columnName).Value;

            foreach (var value in values)
            {
                if (Equals(value, actualValue))
                    return;
            }

            _isMatch = false;
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
            var columnValue = _row.ColumnValues.Single(x => x.ColumnName == columnName).Value;

            if (columnValue is Enum)
                columnValue = (int)columnValue;
            if (value is Enum)
                value = (int)value;

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
            var searchString = expression.SearchString;
            var columnName = expression.PropertyExpression.PropertyName;
            var actualValue = (string)_row.GetColumnValue(columnName).Value;

            Func<string, bool> operation;

            if (expression.HasStartingWildCard && expression.HasEndingWildCard)
            {
                operation = actualValue.Contains;
            }
            else if (expression.HasStartingWildCard)
            {
                operation = actualValue.EndsWith;
            }
            else if (expression.HasEndingWildCard)
            {
                operation = actualValue.StartsWith;
            }
            else
            {
                throw new WeenyMapperException("Invalid like expression for column '{0}' and search string '{1}'", columnName, searchString);
            }

            if (!operation(searchString))
                _isMatch = false;
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
            var matcher = new InMemoryRowMatcher(_row, expression.Expression, _tableIdentifier);

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
using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class OrderByClause : SqlClauseBase
    {
        private readonly Func<string, string> _escape;
        private string _orderByString;

        public OrderByClause(IEnumerable<OrderByStatement> orderByStatements, Func<string, string> escape, string tableName = "")
        {
            _escape = escape;
            _orderByString = string.Join(", ", orderByStatements.Select(x => CreateOrderByString(x, tableName)));
        }

        private string CreateOrderByString(OrderByStatement orderByStatement, string tableName)
        {
            var direction = orderByStatement.Direction == OrderByDirection.Ascending ? "" : " DESC";
            var columnName = CreateColumnNameString(orderByStatement.PropertyName, tableName);

            return columnName + direction;
        }

        private string CreateColumnNameString(string columnName, string tableName)
        {
            var columnNameString = _escape(columnName);

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                columnNameString = _escape(tableName) + "." + columnNameString;
            }

            return columnNameString;
        }

        public override string CommandString
        {
            get { return "ORDER BY " + _orderByString; }
        }

        protected override bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(_orderByString); }
        }
    }
}
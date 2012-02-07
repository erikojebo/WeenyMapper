using System;
using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.QueryParsing;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class OrderByClause : SqlClauseBase
    {
        private readonly Func<string, string> _escape;
        private readonly string _orderByString;

        public OrderByClause(IEnumerable<OrderByStatement> orderByStatements, Func<string, string> escape, string tableName = "")
        {
            _escape = escape;
            _orderByString = string.Join(", ", orderByStatements.Select(x => CreateOrderByString(x, tableName)));
        }

        private string CreateOrderByString(OrderByStatement orderByStatement, string tableName)
        {
            var direction = orderByStatement.Direction == OrderByDirection.Ascending ? "" : " DESC";
            var columnReference = new ColumnReference(orderByStatement.PropertyName, tableName, _escape);

            return columnReference + direction;
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
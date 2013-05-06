using System;
using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.QueryParsing;
using System.Linq;
using WeenyMapper.Extensions;

namespace WeenyMapper.Sql
{
    public class OrderByClause : SqlClauseBase
    {
        private readonly string _orderByString;

        private OrderByClause(string orderByString)
        {
            _orderByString = orderByString;
        }

        public OrderByClause(OrderByStatement orderByStatement, Func<string, string> escape, string tableName = "")
            : this(orderByStatement.AsList(), escape, tableName)
        {
        }

        public OrderByClause(IEnumerable<OrderByStatement> orderByStatements, Func<string, string> escape, string tableName = "")
        {
            _orderByString = string.Join(", ", orderByStatements.Select(x => CreateOrderByString(x, tableName, escape)));
        }

        private string CreateOrderByString(OrderByStatement orderByStatement, string tableName, Func<string, string> escape)
        {
            var direction = orderByStatement.Direction == OrderByDirection.Ascending ? "" : " DESC";
            var columnReference = new ColumnReference(orderByStatement.PropertyName, tableName, escape);

            return columnReference + direction;
        }

        public OrderByClause Combine(OrderByClause orderByClause)
        {
            if (_orderByString.IsNullOrWhiteSpace())
                return new OrderByClause(orderByClause._orderByString);

            var combinedOrderByString = _orderByString + ", " + orderByClause._orderByString;

            return new OrderByClause(combinedOrderByString);
        }

        public override string CommandString
        {
            get { return "ORDER BY " + _orderByString; }
        }

        protected override bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(_orderByString); }
        }

        public static OrderByClause CreateEmpty()
        {
            return new OrderByClause("");
        }
    }
}
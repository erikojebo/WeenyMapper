using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution.InMemory
{
    internal class InMemoryRowSorter : IComparer<Row>
    {
        private readonly ObjectQuery _query;
        private readonly SqlQuery _sqlQuery;
        private readonly IConventionReader _conventionReader;

        public InMemoryRowSorter(ObjectQuery query, SqlQuery sqlQuery, IConventionReader conventionReader)
        {
            _query = query;
            _sqlQuery = sqlQuery;
            _conventionReader = conventionReader;
        }

        public int Compare(Row left, Row right)
        {
            var translatedOrderByStatements = _query.OrderByStatements.Select(orderBy =>
                {
                    var subQuery = _query.SubQueries.First(query => query.OrderByStatements.Contains(orderBy));
                    return new OrderByWithTable
                        {
                            TableIdentifier = subQuery.Alias ?? _conventionReader.GetTableName(subQuery.ResultType),
                            TranslatedOrderBy = orderBy.Translate(_conventionReader)
                        };

                }).ToList();

            if (IsUnorderedPagingQuery())
            {
                AddOrderByStatementForPrimaryKeyColumn(translatedOrderByStatements);
            }

            foreach (var orderByWithTable in translatedOrderByStatements)
            {
                var leftValue = left.GetColumnValue(orderByWithTable.TableIdentifier, orderByWithTable.TranslatedOrderBy.PropertyName).Value;
                var rightValue = right.GetColumnValue(orderByWithTable.TableIdentifier, orderByWithTable.TranslatedOrderBy.PropertyName).Value;

                if (leftValue is IComparable)
                {
                    var leftComparable = (IComparable)leftValue;

                    var result = leftComparable.CompareTo(rightValue);

                    var areDifferent = result != 0;

                    if (areDifferent && orderByWithTable.TranslatedOrderBy.Direction == OrderByDirection.Ascending)
                        return result;
                    if (areDifferent && orderByWithTable.TranslatedOrderBy.Direction == OrderByDirection.Descending)
                        return -1 * result;
                }
            }

            return 0;
        }

        private void AddOrderByStatementForPrimaryKeyColumn(List<OrderByWithTable> orderByStatements)
        {
            var primaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName(_query.SubQueries.First().ResultType);
            orderByStatements.Add(new OrderByWithTable { TableIdentifier = null, TranslatedOrderBy = new OrderByStatement(primaryKeyColumnName)});
        }

        private bool IsUnorderedPagingQuery()
        {
            return _query.OrderByStatements.IsEmpty() && _sqlQuery.IsPagingQuery;
        }

        private class OrderByWithTable
        {
            public string TableIdentifier { get; set; }
            public OrderByStatement TranslatedOrderBy { get; set; }
        }
    }
}
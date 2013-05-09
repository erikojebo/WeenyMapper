using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IConventionReader _conventionReader;

        public InMemoryRowSorter(ObjectQuery query, IConventionReader conventionReader)
        {
            _query = query;
            _conventionReader = conventionReader;
        }

        public int Compare(Row left, Row right)
        {
            var translatedOrderByStatements = _query.OrderByStatements.Select(x => x.Translate(_conventionReader)).ToList();

            if (IsUnorderedPagingQuery())
            {
                AddOrderByStatementForPrimaryKeyColumn(translatedOrderByStatements);
            }

            foreach (var translatedOrderBy in translatedOrderByStatements)
            {
                var leftValue = left.ColumnValues.First(x => x.ColumnName == translatedOrderBy.PropertyName).Value;
                var rightValue = right.ColumnValues.First(x => x.ColumnName == translatedOrderBy.PropertyName).Value;

                if (leftValue is IComparable)
                {
                    var leftComparable = (IComparable)leftValue;

                    var result = leftComparable.CompareTo(rightValue);

                    var areDifferent = result != 0;

                    if (areDifferent && translatedOrderBy.Direction == OrderByDirection.Ascending)
                        return result;
                    if (areDifferent && translatedOrderBy.Direction == OrderByDirection.Descending)
                        return -1 * result;
                }
            }

            return 0;
        }

        private void AddOrderByStatementForPrimaryKeyColumn(List<OrderByStatement> orderByStatements)
        {
            var primaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName(_query.SubQueries.First().ResultType);
            orderByStatements.Add(new OrderByStatement(primaryKeyColumnName));
        }

        private bool IsUnorderedPagingQuery()
        {
            return _query.OrderByStatements.IsEmpty() && _query.SubQueries.First().IsPagingQuery;
        }

    }
}
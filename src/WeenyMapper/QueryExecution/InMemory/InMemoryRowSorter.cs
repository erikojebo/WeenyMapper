using System;
using System.Collections.Generic;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution.InMemory
{
    internal class InMemoryRowSorter : IComparer<Row>
    {
        private readonly SqlQuery _sqlQuery;

        public InMemoryRowSorter(SqlQuery sqlQuery)
        {
            _sqlQuery = sqlQuery;
        }

        public int Compare(Row left, Row right)
        {
            foreach (var orderByStatement in _sqlQuery.OrderByStatements)
            {
                var leftValue = left.GetColumnValue(orderByStatement.TableIdentifier, orderByStatement.PropertyName).Value;
                var rightValue = right.GetColumnValue(orderByStatement.TableIdentifier, orderByStatement.PropertyName).Value;

                if (leftValue is IComparable)
                {
                    var leftComparable = (IComparable)leftValue;

                    var result = leftComparable.CompareTo(rightValue);

                    var areDifferent = result != 0;

                    if (areDifferent && orderByStatement.Direction == OrderByDirection.Ascending)
                        return result;
                    if (areDifferent && orderByStatement.Direction == OrderByDirection.Descending)
                        return -1 * result;
                }
            }

            return 0;
        }
    }
}
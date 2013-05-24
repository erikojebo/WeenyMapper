using System;
using System.Collections.Generic;
using WeenyMapper.Exceptions;
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
                var leftColumnValue = left.GetColumnValue(orderByStatement.TableIdentifier, orderByStatement.PropertyName);
                var rightColumnValue = right.GetColumnValue(orderByStatement.TableIdentifier, orderByStatement.PropertyName);

                if (leftColumnValue == null || rightColumnValue == null)
                    throw new WeenyMapperException("Could not add Order By statement for the table with name or alias '{0}'. Did you forget to specify the alias for the order by statement? Or perhaps forgot to specify an alias when joining?", orderByStatement.TableIdentifier);

                var leftValue = leftColumnValue.Value;
                var rightValue = rightColumnValue.Value;

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
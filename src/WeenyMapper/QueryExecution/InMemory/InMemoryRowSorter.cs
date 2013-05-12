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
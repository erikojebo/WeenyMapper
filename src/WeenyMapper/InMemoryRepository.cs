using System;
using WeenyMapper.Exceptions;

namespace WeenyMapper
{
    public class InMemoryRepository : Repository
    {
        public override QueryExecution.CustomSqlQueryExecutor<T> FindBySql<T>(System.Data.Common.DbCommand dbCommand)
        {
            throw new WeenyMapperException("The InMemoryRepository does not support custom SQL queries");
        }

    }
}
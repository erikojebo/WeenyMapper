using System.Data.Common;
using WeenyMapper.Exceptions;
using WeenyMapper.QueryExecution;

namespace WeenyMapper
{
    public class InMemoryRepository : Repository
    {
        public override CustomSqlQueryExecutor<T> FindBySql<T>(DbCommand dbCommand)
        {
            throw new WeenyMapperException("The InMemoryRepository does not support custom SQL queries");
        }

        protected override IObjectQueryExecutor CreateObjectQueryExecutor<T>()
        {
            return new InMemoryObjectQueryExecutor();
        }

        protected override IObjectCountExecutor CreateObjectCountExecutor<T>()
        {
            return new InMemoryObjectCountExecutor();
        }

        protected override IObjectDeleteExecutor CreateObjectDeleteExecutor<T>()
        {
            return new InMemoryObjectDeleteExecutor();
        }

        protected override IObjectInsertExecutor CreateObjectInsertExecutor<T>()
        {
            return new InMemoryObjectInsertExecutor();
        }

        protected override IObjectUpdateExecutor CreateObjectUpdateExecutor<T>()
        {
            return new InMemoryObjectUpdateExecutor();
        }
    }
}
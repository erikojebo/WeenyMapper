using System.Data.Common;
using WeenyMapper.Exceptions;
using WeenyMapper.QueryExecution;

namespace WeenyMapper
{
    public class InMemoryRepository : Repository
    {
        private InMemoryDatabase _inMemoryDatabase = new InMemoryDatabase();

        public override CustomSqlQueryExecutor<T> FindBySql<T>(DbCommand dbCommand)
        {
            throw new WeenyMapperException("The InMemoryRepository does not support custom SQL queries");
        }

        protected override IObjectQueryExecutor CreateObjectQueryExecutor<T>()
        {
            return new InMemoryObjectQueryExecutor(_inMemoryDatabase);
        }

        protected override IObjectCountExecutor CreateObjectCountExecutor<T>()
        {
            return new InMemoryObjectCountExecutor(_inMemoryDatabase);
        }

        protected override IObjectDeleteExecutor CreateObjectDeleteExecutor<T>()
        {
            return new InMemoryObjectDeleteExecutor(_inMemoryDatabase);
        }

        protected override IObjectInsertExecutor CreateObjectInsertExecutor<T>()
        {
            return new InMemoryObjectInsertExecutor(_inMemoryDatabase);
        }

        protected override IObjectUpdateExecutor CreateObjectUpdateExecutor<T>()
        {
            return new InMemoryObjectUpdateExecutor(_inMemoryDatabase);
        }
    }
}
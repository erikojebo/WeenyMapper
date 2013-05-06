using System.Data.Common;
using WeenyMapper.Exceptions;
using WeenyMapper.Mapping;
using WeenyMapper.QueryExecution;

namespace WeenyMapper
{
    public class InMemoryRepository : Repository
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryRepository()
        {
            var conventionReader = CreateConventionReader();

            _inMemoryDatabase = new InMemoryDatabase(conventionReader, new EntityMapper(conventionReader));
        }

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
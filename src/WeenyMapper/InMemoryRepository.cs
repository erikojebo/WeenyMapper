using System.Data.Common;
using WeenyMapper.Exceptions;
using WeenyMapper.Mapping;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryExecution.InMemory;

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
            throw new WeenyMapperException("The InMemoryRepository does not support custom SQL queries, since that would require writing a complete in-memory SQL database, which is kind of out of scope :)");
        }

        protected override IObjectQueryExecutor CreateObjectQueryExecutor<T>()
        {
            UseNewConventionReaderForCurrentConvention();
            return new InMemoryObjectQueryExecutor(_inMemoryDatabase);
        }

        protected override IObjectCountExecutor CreateObjectCountExecutor<T>()
        {
            UseNewConventionReaderForCurrentConvention();
            return new InMemoryObjectCountExecutor(_inMemoryDatabase);
        }

        protected override IObjectDeleteExecutor CreateObjectDeleteExecutor<T>()
        {
            UseNewConventionReaderForCurrentConvention();
            return new InMemoryObjectDeleteExecutor(_inMemoryDatabase);
        }

        protected override IObjectInsertExecutor CreateObjectInsertExecutor<T>()
        {
            UseNewConventionReaderForCurrentConvention();
            return new InMemoryObjectInsertExecutor(_inMemoryDatabase);
        }

        protected override IObjectUpdateExecutor CreateObjectUpdateExecutor<T>()
        {
            UseNewConventionReaderForCurrentConvention();
            return new InMemoryObjectUpdateExecutor(_inMemoryDatabase);
        }

        private void UseNewConventionReaderForCurrentConvention()
        {
            var conventionReader = CreateConventionReader();
            _inMemoryDatabase.ConventionReader = conventionReader;
            _inMemoryDatabase.EntityMapper = CreateEntityMapper();
        }
    }
}
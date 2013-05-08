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
            UpdateConventionReader();
            return new InMemoryObjectQueryExecutor(_inMemoryDatabase);
        }

        protected override IObjectCountExecutor CreateObjectCountExecutor<T>()
        {
            UpdateConventionReader();
            return new InMemoryObjectCountExecutor(_inMemoryDatabase);
        }

        protected override IObjectDeleteExecutor CreateObjectDeleteExecutor<T>()
        {
            UpdateConventionReader();
            return new InMemoryObjectDeleteExecutor(_inMemoryDatabase);
        }

        protected override IObjectInsertExecutor CreateObjectInsertExecutor<T>()
        {
            UpdateConventionReader();
            return new InMemoryObjectInsertExecutor(_inMemoryDatabase);
        }

        protected override IObjectUpdateExecutor CreateObjectUpdateExecutor<T>()
        {
            UpdateConventionReader();
            return new InMemoryObjectUpdateExecutor(_inMemoryDatabase);
        }

        private void UpdateConventionReader()
        {
            var conventionReader = CreateConventionReader();
            _inMemoryDatabase.ConventionReader = conventionReader;
            _inMemoryDatabase.EntityMapper = new EntityMapper(conventionReader);
        }
    }
}
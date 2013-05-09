using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution.InMemory
{
    public class InMemoryObjectCountExecutor : IObjectCountExecutor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryObjectCountExecutor(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public string ConnectionString { get; set; }

        public int Count<T>(QueryExpression queryExpression)
        {
            return _inMemoryDatabase.Count<T>(queryExpression);
        }
    }
}
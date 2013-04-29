using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
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
            throw new System.NotImplementedException();
        }
    }
}
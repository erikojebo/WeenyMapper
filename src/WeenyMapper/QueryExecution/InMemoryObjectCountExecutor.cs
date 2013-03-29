using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectCountExecutor : IObjectCountExecutor
    {
        public string ConnectionString { get; set; }
        public int Count<T>(QueryExpression queryExpression)
        {
            throw new System.NotImplementedException();
        }
    }
}
using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        T Find<T>(string className, IDictionary<string, object> constraints) where T : new();
        string ConnectionString { get; set; }
    }
}
using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectCountExecutor
    {
        string ConnectionString { get; set; }
        int Count<T>(IDictionary<string, object> constraints);
    }
}
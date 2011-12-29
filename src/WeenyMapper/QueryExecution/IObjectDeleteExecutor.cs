using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectDeleteExecutor
    {
        string ConnectionString { get; set; }
        int Delete<T>(T instance);
        int Delete<T>(IDictionary<string, object> constraints);
    }
}
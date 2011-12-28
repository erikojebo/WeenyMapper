using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectDeleteExecutor
    {
        string ConnectionString { get; set; }
        void Delete<T>(T instance);
        void Delete<T>(IDictionary<string, object> constraints);
    }
}
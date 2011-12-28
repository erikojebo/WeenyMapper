using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectUpdateExecutor
    {
        string ConnectionString { get; set; }
        void Update<T>(T instance);
        void Update<T>(IDictionary<string, object> constraints, IDictionary<string, object> setters);
    }
}
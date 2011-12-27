using System.Collections;
using System.Collections.Generic;

namespace WeenyMapper.SqlGeneration
{
    public interface IObjectQueryExecutor
    {
        T Find<T>(string className, IDictionary<string, object> constraints) where T : new();
        string ConnectionString { get; set; }
    }
}
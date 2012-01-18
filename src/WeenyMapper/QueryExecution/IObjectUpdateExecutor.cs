using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectUpdateExecutor
    {
        string ConnectionString { get; set; }
        int Update<T>(T instance);
        int Update<T>(IDictionary<string, object> constraints, IDictionary<string, object> setters);
        int Update<T>(QueryExpression queryExpression, IDictionary<string, object> setters);
    }
}
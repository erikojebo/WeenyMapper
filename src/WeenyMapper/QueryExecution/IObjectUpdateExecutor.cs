using System.Collections.Generic;
using System.Reflection;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectUpdateExecutor
    {
        string ConnectionString { get; set; }
        int Update<T>(T instance);
        int Update<T>(QueryExpression queryExpression, IDictionary<PropertyInfo, object> setters);
    }
}
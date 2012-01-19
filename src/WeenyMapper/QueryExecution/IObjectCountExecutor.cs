using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectCountExecutor
    {
        string ConnectionString { get; set; }
        int Count<T>(QueryExpression queryExpression);
    }
}
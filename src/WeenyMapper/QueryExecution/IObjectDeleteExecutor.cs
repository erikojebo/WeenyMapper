using System;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectDeleteExecutor
    {
        string ConnectionString { get; set; }
        void Delete<T>(T instance);
        void DeleteAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null);
        int Delete<T>(QueryExpression queryExpression);
    }
}
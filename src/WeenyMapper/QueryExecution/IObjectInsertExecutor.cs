using System;
using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectInsertExecutor
    {
        string ConnectionString { get; set; }
        void Insert<T>(IEnumerable<T> entities);
        void InsertAsync<T>(IEnumerable<T> entities, Action callback, Action<Exception> errorCallback = null);
    }
}
using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }

        IList<T> Find<T>(ObjectQuery query) where T : new();
        TScalar FindScalar<T, TScalar>(ObjectQuery query);
        IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query);
    }
}
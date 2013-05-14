using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }

        IList<T> Find<T>(SqlQuery sqlQuery) where T : new();
        TScalar FindScalar<T, TScalar>(SqlQuery sqlQuery);
        IList<TScalar> FindScalarList<T, TScalar>(SqlQuery sqlQuery);
    }
}
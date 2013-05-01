using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }

        TScalar FindScalar<T, TScalar>(AliasedObjectSubQuery subQuery);
        IList<TScalar> FindScalarList<T, TScalar>(AliasedObjectSubQuery subQuery);
        IList<T> Find<T>(ObjectQuery query) where T : new();
    }
}
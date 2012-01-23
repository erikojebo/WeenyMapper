using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }

        IList<T> Find<T>(ObjectQuerySpecification<T> querySpecification) where T : new();
        TScalar FindScalar<T, TScalar>(ObjectQuerySpecification<T> querySpecification);
        IList<TScalar> FindScalarList<T, TScalar>(ObjectQuerySpecification<T> querySpecification);
    }
}
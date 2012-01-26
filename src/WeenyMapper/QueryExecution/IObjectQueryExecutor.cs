using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }

        IList<T> Find<T>(ObjectQuerySpecification querySpecification) where T : new();
        TScalar FindScalar<T, TScalar>(ObjectQuerySpecification querySpecification);
        IList<TScalar> FindScalarList<T, TScalar>(ObjectQuerySpecification querySpecification);
    }
}
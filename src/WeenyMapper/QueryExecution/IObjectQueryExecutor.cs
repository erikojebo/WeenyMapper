using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }

        IList<T> Find<T>(QuerySpecification querySpecification) where T : new();
        TScalar FindScalar<T, TScalar>(QuerySpecification querySpecification);
        IList<TScalar> FindScalarList<T, TScalar>(QuerySpecification querySpecification);
    }
}
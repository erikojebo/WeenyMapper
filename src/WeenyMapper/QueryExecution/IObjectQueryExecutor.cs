using System.Collections;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }
        
        IList<T> Find<T>(string className, IDictionary<string, object> constraints) where T : new();
        IList<T> Find<T>(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect) where T : new();
        IList<T> Find<T>(string className, QueryExpression queryExpression) where T : new();

        TScalar FindScalar<T, TScalar>(string className, IDictionary<string, object> constraints);
        TScalar FindScalar<T, TScalar>(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect);

        IList<TScalar> FindScalarList<T, TScalar>(string className, IDictionary<string, object> constraints);
        IList<TScalar> FindScalarList<T, TScalar>(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect);
    }
}
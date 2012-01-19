using System.Collections;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public interface IObjectQueryExecutor
    {
        string ConnectionString { get; set; }
        
        IList<T> Find<T>(string className, QueryExpression queryExpression, IEnumerable<string> propertiesToSelect) where T : new();
        IList<T> Find<T>(string className, QueryExpression queryExpression) where T : new();

        TScalar FindScalar<T, TScalar>(string className, QueryExpression queryExpression);
        TScalar FindScalar<T, TScalar>(string className, QueryExpression queryExpression, IEnumerable<string> propertiesToSelect);

        IList<TScalar> FindScalarList<T, TScalar>(string className, QueryExpression queryExpression);
        IList<TScalar> FindScalarList<T, TScalar>(string className, QueryExpression queryExpression, IEnumerable<string> propertiesToSelect);
    }
}
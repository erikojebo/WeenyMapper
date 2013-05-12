using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class AliasedObjectSubQuery
    {
        public AliasedObjectSubQuery(Type resultType)
        {
            OrderByStatements = new List<OrderByStatement>();
            ResultType = resultType;
        }

        public IList<OrderByStatement> OrderByStatements { get; set; }
        public Type ResultType { get; set; }
        public string Alias { get; set; }
    }
}
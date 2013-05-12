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
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }
        public Type ResultType { get; set; }
        public string Alias { get; set; }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }
    }
}
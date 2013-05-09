using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;

namespace WeenyMapper.Sql
{
    public class AliasedObjectSubQuery
    {
        public AliasedObjectSubQuery(Type resultType)
        {
            PropertiesToSelect = new List<string>();
            OrderByStatements = new List<OrderByStatement>();
            ResultType = resultType;
        }

        public IList<string> PropertiesToSelect { get; set; }
        public IList<OrderByStatement> OrderByStatements { get; set; }
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }
        public Type ResultType { get; set; }
        public string Alias { get; set; }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public IList<string> GetColumnNamesToSelect(IConventionReader conventionReader)
        {
            if (!PropertiesToSelect.Any())
            {
                return conventionReader.GetSelectableColumNames(ResultType).ToList();
            }

            return PropertiesToSelect.Select(x => conventionReader.GetColumnName(x, ResultType)).ToList();
        }
    }
}
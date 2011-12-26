using System;

namespace WeenyMapper.QueryParsing
{
    public class QueryParser : IQueryParser
    {
        public SelectQuery ParseSelectQuery(string methodName)
        {
            var parts = methodName.Split(new[] { "By" }, StringSplitOptions.None);

            var className = parts[0];
            var propertyNameString = parts[1];

            var propertyNames = propertyNameString.Split(new[] { "And" }, StringSplitOptions.None);

            var selectQuery = new SelectQuery();

            selectQuery.ClassName = className;

            foreach (var propertyName in propertyNames)
            {
                selectQuery.ConstraintProperties.Add(propertyName);                
            }

            return selectQuery;
        }
    }
}
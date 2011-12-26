using System;
using System.Text.RegularExpressions;

namespace WeenyMapper.QueryParsing
{
    public class QueryParser : IQueryParser
    {
        public SelectQuery ParseSelectQuery(string methodName)
        {
            var regex = new Regex("(?<className>.*)By(?<propertyName>.*)");
            var match = regex.Match(methodName);

            var className = match.Groups["className"].Value;
            var propertyName = match.Groups["propertyName"].Value;

            var selectQuery = new SelectQuery();

            selectQuery.ClassName = className;
            selectQuery.ConstraintProperties.Add(propertyName);

            return selectQuery;
        }
    }
}
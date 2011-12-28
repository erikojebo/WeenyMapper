using System;
using System.Collections.Generic;

namespace WeenyMapper.QueryParsing
{
    public class QueryParser : IQueryParser
    {
        public IList<string> GetConstraintProperties(string methodName)
        {
            var propertyNameString = methodName.Substring("By".Length);

            var propertyNames = propertyNameString.Split(new[] { "And" }, StringSplitOptions.None);

            var constraintProperties = new List<string>();

            foreach (var propertyName in propertyNames)
            {
                constraintProperties.Add(propertyName);                
            }

            return constraintProperties;
        }
    }
}
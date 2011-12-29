using System.Collections.Generic;

namespace WeenyMapper.QueryParsing
{
    public interface IQueryParser
    {
        IList<string> GetConstraintProperties(string methodName, string methodPrefix);
    }
}
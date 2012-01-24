using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Extensions
{
    public static class SqlCommandExtensions
    {
        public static IList<DbParameter> SortByParameterName(this DbParameterCollection parameters)
        {
            return parameters.OfType<DbParameter>().OrderBy(x => x.ParameterName).ToList();
        }
    }
}
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace WeenyMapper.Extensions
{
    public static class SqlCommandExtensions
    {
        public static void DisposeAll(this IEnumerable<SqlCommand> commands)
        {
            foreach (var sqlCommand in commands)
            {
                sqlCommand.Dispose();
            }
        }

        public static IList<DbParameter> SortByParameterName(this DbParameterCollection parameters)
        {
            return parameters.OfType<DbParameter>().OrderBy(x => x.ParameterName).ToList();
        }
    }
}
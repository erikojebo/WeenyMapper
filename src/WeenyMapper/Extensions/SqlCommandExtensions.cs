using System.Collections.Generic;
using System.Data.SqlClient;

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
    }
}
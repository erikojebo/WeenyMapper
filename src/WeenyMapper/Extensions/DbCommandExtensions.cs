using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace WeenyMapper.Extensions
{
    public static class DbCommandExtensions
    {
        public static void ExecuteNonQuery(this DbCommand command, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                command.Connection = connection;

                command.ExecuteNonQuery();

                connection.Dispose();
            }
        }

        public static void DisposeAll(this IEnumerable<SqlCommand> commands)
        {
            foreach (var sqlCommand in commands)
            {
                sqlCommand.Dispose();
            }
        }
    }
}
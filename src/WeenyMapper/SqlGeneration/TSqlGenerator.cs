using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace WeenyMapper.SqlGeneration
{
    public class TSqlGenerator : ISqlGenerator
    {
        public SqlCommand GenerateSelectQuery(string tableName, IDictionary<string, object> constraints)
        {
            var commandString = "select * from " + tableName;

            if (constraints.Any())
            {
                commandString += " " + CreateWhereClause(constraints);
            }

            return new SqlCommand(commandString);
        }

        private string CreateWhereClause(IDictionary<string, object> constraints)
        {
            var whereClause = string.Format("where {0} = '{1}'", constraints.First().Key, constraints.First().Value);

            return whereClause;
        }
    }
}
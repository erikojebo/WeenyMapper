using System;
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

        public SqlCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues)
        {
            // SQL injection alert here, but simplest possible to get the first acceptance test passing

            var columnNames = string.Join(", ", propertyValues.Keys);
            var quotedValues = propertyValues.Values.Select(x => "'" + x + "'");
            var columnValues = string.Join(", ", quotedValues);

            var insertCommand = string.Format("insert into {0} ({1}) values ({2})", tableName, columnNames, columnValues);

            return new SqlCommand(insertCommand);
        }
    }
}
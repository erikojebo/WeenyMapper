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
            var commandString = "select * from " + Escape(tableName);

            if (constraints.Any())
            {
                commandString += " " + CreateWhereClause(constraints);
            }

            return new SqlCommand(commandString);
        }

        private string CreateWhereClause(IDictionary<string, object> constraints)
        {
            var columnName = Escape(constraints.First().Key);
            var value = constraints.First().Value;

            var whereClause = string.Format("where {0} = '{1}'", columnName, value);

            return whereClause;
        }

        public SqlCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues)
        {
            // SQL injection alert here, but simplest possible to get the first acceptance test passing

            var escapedColumnNames = propertyValues.Keys.Select(Escape);
            var columnNamesString = string.Join(", ", escapedColumnNames);
            var quotedValues = propertyValues.Values.Select(x => "'" + x + "'");
            var columnValues = string.Join(", ", quotedValues);

            var escapedTableName = Escape(tableName);

            var insertCommand = string.Format("insert into {0} ({1}) values ({2})", escapedTableName, columnNamesString, columnValues);

            return new SqlCommand(insertCommand);
        }

        private string Escape(string propertyName)
        {
            return string.Format("[{0}]", propertyName);
        }
    }
}
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

            var sqlCommand = new SqlCommand(commandString);

            foreach (var constraint in constraints)
            {
                sqlCommand.Parameters.Add(new SqlParameter(constraint.Key, constraint.Value));
            }

            return sqlCommand;
        }

        private string CreateWhereClause(IDictionary<string, object> constraints)
        {
            var whereClause = "where ";

            for (int i = 0; i < constraints.Count; i++)
            {
                var columnName = constraints.ElementAt(i).Key;

                whereClause += string.Format("{0} = @{1}", Escape(columnName), columnName);

                var isLastConstraint = i == constraints.Count - 1;

                if (!isLastConstraint)
                {
                    whereClause += " and ";
                }
            }

            return whereClause;
        }

        public SqlCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues)
        {
            var escapedColumnNames = propertyValues.Keys.Select(Escape);
            var columnNamesString = string.Join(", ", escapedColumnNames);

            var parameterNames = propertyValues.Select(x => "@" + x.Key);
            var parameterNamesString = string.Join(", ", parameterNames);

            var escapedTableName = Escape(tableName);

            var insertCommand = string.Format("insert into {0} ({1}) values ({2})", escapedTableName, columnNamesString, parameterNamesString);

            var sqlCommand = new SqlCommand(insertCommand);

            foreach (var propertyValue in propertyValues)
            {
                sqlCommand.Parameters.Add(new SqlParameter(propertyValue.Key, propertyValue.Value));
            }

            return sqlCommand;
        }

        private string Escape(string propertyName)
        {
            return string.Format("[{0}]", propertyName);
        }
    }
}
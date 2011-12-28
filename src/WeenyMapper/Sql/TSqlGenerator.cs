using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class TSqlGenerator : ISqlGenerator
    {
        public DbCommand GenerateSelectQuery(string tableName, IEnumerable<string> columnNamesToSelect, IDictionary<string, object> constraints)
        {
            var escapedColumnNamesToSelect = columnNamesToSelect.Select(Escape);
            var selectedColumnString = string.Join(", ", escapedColumnNamesToSelect);
            var commandString = string.Format("select {0} from {1}", selectedColumnString, Escape(tableName));

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

        public DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues)
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

        public DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> propertyValues)
        {
            var setters = new Dictionary<string, object>();
            var constraints = new Dictionary<string, object>();

            foreach (var propertyValue in propertyValues)
            {
                if (propertyValue.Key == primaryKeyColumn)
                {
                    constraints.Add(propertyValue.Key, propertyValue.Value);                    
                }
                else
                {
                    setters.Add(propertyValue.Key, propertyValue.Value);
                }
            }

            return CreateUpdateCommand(tableName, primaryKeyColumn, constraints, setters);
        }

        public DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> columnConstraints, IDictionary<string, object> columnSetters)
        {
            var updateStatements = columnSetters.Where(x => x.Key != primaryKeyColumn)
                .Select(x => string.Format("{0} = @{1}", Escape(x.Key), x.Key));

            var updateString = string.Join(", ", updateStatements);

            var constraintStatements = columnConstraints.Select(x => string.Format("{0} = @{1}Constraint", Escape(x.Key), x.Key));
            var constraintString = string.Join(" and ", constraintStatements);

            var sql = string.Format("update {0} set {1}", Escape(tableName), updateString);

            if (columnConstraints.Any())
            {
                var whereClause = string.Format(" where {0}", constraintString);
                sql += whereClause;
            }

            var updateCommand = new SqlCommand(sql);

            foreach (var propertyValue in columnSetters)
            {
                updateCommand.Parameters.Add(new SqlParameter(propertyValue.Key, propertyValue.Value));
            }

            foreach (var propertyValue in columnConstraints)
            {
                updateCommand.Parameters.Add(new SqlParameter(propertyValue.Key + "Constraint", propertyValue.Value));
            }

            return updateCommand;
        }

        private string Escape(string propertyName)
        {
            return string.Format("[{0}]", propertyName);
        }
    }
}
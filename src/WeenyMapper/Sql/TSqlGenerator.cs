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
                commandString += " where " + CreateConstraintClause(constraints);
            }

            return CreateSqlCommandWithConstraintParameters(commandString, constraints);
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

            var sql = string.Format("update {0} set {1}", Escape(tableName), updateString);

            var command = CreateSqlCommandWithWhereClause(sql, columnConstraints);

            foreach (var propertyValue in columnSetters)
            {
                command.Parameters.Add(new SqlParameter(propertyValue.Key, propertyValue.Value));
            }

            return command;
        }

        public DbCommand CreateDeleteCommand(string tableName, IDictionary<string, object> constraints)
        {
            var deleteCommand = string.Format("delete from {0}", Escape(tableName));

            return CreateSqlCommandWithWhereClause(deleteCommand, constraints);
        }

        public DbCommand CreateCountCommand(string tableName, IDictionary<string, object> columnConstraints)
        {
            var countQuery = string.Format("select count(*) from {0}", Escape(tableName));

            return CreateSqlCommandWithWhereClause(countQuery, columnConstraints);
        }

        private DbCommand CreateSqlCommandWithWhereClause(string sql, IDictionary<string, object> columnConstraints)
        {
            sql = AppendWhereClause(sql, columnConstraints);

            return CreateSqlCommandWithConstraintParameters(sql, columnConstraints);
        }

        private string AppendWhereClause(string countQuery, IDictionary<string, object> columnConstraints)
        {
            if (columnConstraints.Any())
            {
                countQuery += " where " + CreateConstraintClause(columnConstraints);
            }

            return countQuery;
        }

        private SqlCommand CreateSqlCommandWithConstraintParameters(string commandString, IDictionary<string, object> constraints)
        {
            var sqlCommand = new SqlCommand(commandString);

            foreach (var constraint in constraints)
            {
                sqlCommand.Parameters.Add(new SqlParameter(constraint.Key + "Constraint", constraint.Value));
            }

            return sqlCommand;
        }

        private string CreateConstraintClause(IDictionary<string, object> columnConstraints)
        {
            var constraintStrings = columnConstraints
                .Select(x => x.Key)
                .Select(columnName => string.Format("{0} = @{1}Constraint", Escape(columnName), columnName));

            return string.Join(" and ", constraintStrings);
        }

        private string Escape(string propertyName)
        {
            return string.Format("[{0}]", propertyName);
        }
    }
}
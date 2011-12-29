using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;
using System.Linq;

namespace WeenyMapper.QueryExecution
{
    public class ObjectInsertExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionDataReader _conventionDataReader;

        public ObjectInsertExecutor(ISqlGenerator sqlGenerator, IConventionDataReader conventionDataReader)
        {
            _sqlGenerator = sqlGenerator;
            _conventionDataReader = conventionDataReader;
        }

        public string ConnectionString { get; set; }

        public void Insert<T>(IEnumerable<T> entities)
        {
            var commands = new List<DbCommand>();

            foreach (var entity in entities)
            {
                var columnValues = _conventionDataReader.GetColumnValuesFromEntity(entity);
                var tableName = _conventionDataReader.GetTableName<T>();
                var command = _sqlGenerator.CreateInsertCommand(tableName, columnValues);

                commands.Add(command);
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                foreach (var dbCommand in commands)
                {
                    dbCommand.Connection = connection;
                    dbCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
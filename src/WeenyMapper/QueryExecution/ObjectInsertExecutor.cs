using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

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

        public void Insert<T>(T instance)
        {
            var columnValues = _conventionDataReader.GetColumnValuesFromEntity(instance);
            var tableName = _conventionDataReader.GetTableName<T>();
            var command = _sqlGenerator.CreateInsertCommand(tableName, columnValues);

            command.ExecuteNonQuery(ConnectionString);
        }
    }
}
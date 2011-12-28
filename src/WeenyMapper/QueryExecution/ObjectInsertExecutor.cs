using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectInsertExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionalEntityDataReader _entityDataReader;

        public ObjectInsertExecutor(ISqlGenerator sqlGenerator, IConventionalEntityDataReader entityDataReader)
        {
            _sqlGenerator = sqlGenerator;
            _entityDataReader = entityDataReader;
        }

        public string ConnectionString { get; set; }

        public void Insert<T>(T instance)
        {
            var columnValues = _entityDataReader.GetColumnValuesFromEntity(instance);
            var tableName = _entityDataReader.GetTableName<T>();
            var command = _sqlGenerator.CreateInsertCommand(tableName, columnValues);

            command.ExecuteNonQuery(ConnectionString);
        }
    }
}
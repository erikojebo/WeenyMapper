using System.Data.Common;
using System.Data.SqlClient;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryBuilding
{
    public class StaticInsertBuilder<T>
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IPropertyReader _propertyReader;

        public StaticInsertBuilder(IConvention convention, ISqlGenerator sqlGenerator, IPropertyReader propertyReader)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _propertyReader = propertyReader;
        }

        public string ConnectionString { get; set; }

        public void Insert(T instance)
        {
            var columnValues = _propertyReader.GetColumnValues(instance);
            var tableName = _convention.GetTableName(typeof(T).Name);
            var command = _sqlGenerator.CreateInsertCommand(tableName, columnValues);

            command.ExecuteNonQuery(ConnectionString);
        }
    }
}
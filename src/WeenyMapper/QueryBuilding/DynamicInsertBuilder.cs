using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using WeenyMapper.Conventions;
using WeenyMapper.Reflection;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicInsertBuilder : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IPropertyReader _propertyReader;

        public DynamicInsertBuilder() : this(new DefaultConvention(), new TSqlGenerator(), new PropertyReader(new DefaultConvention())) {}

        public DynamicInsertBuilder(IConvention convention, ISqlGenerator sqlGenerator, IPropertyReader propertyReader)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _propertyReader = propertyReader;
        }

        public string ConnectionString { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var className = binder.Name;
            var tableName = _convention.GetTableName(className);

            var objectToInsert = args[0];

            var columnValues = _propertyReader.GetColumnValues(objectToInsert);

            var command = _sqlGenerator.CreateInsertCommand(tableName, columnValues);

            ExecuteCommand(command);

            result = null;

            return true;
        }

        private void ExecuteCommand(DbCommand command)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;

                command.ExecuteNonQuery();

                connection.Dispose();
            }
        }
    }
}
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicUpdateBuilder : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IPropertyReader _propertyReader;

        public DynamicUpdateBuilder() : this(new DefaultConvention(), new TSqlGenerator(), new PropertyReader(new DefaultConvention())) {}

        public DynamicUpdateBuilder(IConvention convention, ISqlGenerator sqlGenerator, IPropertyReader propertyReader)
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
            
            var idProperty = objectToInsert.GetType().GetProperties()
                .Select(x => x.Name)
                .First(_convention.IsIdProperty); 

            var primaryKeyColumn = _convention.GetColumnName(idProperty);

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnValues);

            command.ExecuteNonQuery(ConnectionString);

            result = null;

            return true;
        }
    }
}
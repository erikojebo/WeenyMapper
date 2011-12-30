using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.Mapping;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectQueryExecutor : IObjectQueryExecutor
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private readonly IEntityMapper _entityMapper;

        public ObjectQueryExecutor(IConvention convention, ISqlGenerator sqlGenerator, IDbCommandExecutor dbCommandExecutor, IEntityMapper entityMapper)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _dbCommandExecutor = dbCommandExecutor;
            _entityMapper = entityMapper;
        }

        public string ConnectionString { get; set; }

        public IList<T> Find<T>(string className, IDictionary<string, object> constraints) where T : new()
        {
            var propertiesInTargetType = typeof(T).GetProperties().Select(x => x.Name);

            return Find<T>(className, constraints, propertiesInTargetType);
        }

        public IList<T> Find<T>(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect) where T : new()
        {
            var columnNamesToSelect = propertiesToSelect.Select(_convention.GetColumnName);
            var tableName = _convention.GetTableName(className);

            var columnConstraints = constraints.TransformKeys(_convention.GetColumnName);

            var command = _sqlGenerator.GenerateSelectQuery(tableName, columnNamesToSelect, columnConstraints);

            return ReadEntities<T>(command);
        }

        private IList<T> ReadEntities<T>(DbCommand command) where T : new()
        {
            return _dbCommandExecutor.ExecuteQuery(command, _entityMapper.CreateInstance<T>, ConnectionString);
        }

        
    }
}
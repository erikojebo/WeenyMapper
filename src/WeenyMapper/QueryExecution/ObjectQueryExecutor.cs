using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

        public TScalar FindScalar<T, TScalar>(string className, IDictionary<string, object> constraints)
        {
            var propertiesInTargetType = GetPropertiesInTargetType<T>();

            return FindScalar<T, TScalar>(className, constraints, propertiesInTargetType);
        }

        public TScalar FindScalar<T, TScalar>(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect)
        {
            var command = CreateCommand(className, constraints, propertiesToSelect);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }
        
        public IList<TScalar> FindScalarList<T, TScalar>(string className, IDictionary<string, object> constraints)
        {
            var propertiesInTargetType = GetPropertiesInTargetType<T>();

            return FindScalarList<T, TScalar>(className, constraints, propertiesInTargetType);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect)
        {
            var command = CreateCommand(className, constraints, propertiesToSelect);

            return _dbCommandExecutor.ExecuteScalarList<TScalar>(command, ConnectionString);
        }

        public IList<T> Find<T>(string className, IDictionary<string, object> constraints) where T : new()
        {
            var propertiesInTargetType = GetPropertiesInTargetType<T>();

            return Find<T>(className, constraints, propertiesInTargetType);
        }

        public IList<T> Find<T>(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect) where T : new()
        {
            var command = CreateCommand(className, constraints, propertiesToSelect);

            return ReadEntities<T>(command);
        }

        private DbCommand CreateCommand(string className, IDictionary<string, object> constraints, IEnumerable<string> propertiesToSelect)
        {
            var columnNamesToSelect = propertiesToSelect.Select(_convention.GetColumnName);
            var tableName = _convention.GetTableName(className);

            var columnConstraints = constraints.TransformKeys(_convention.GetColumnName);

            return _sqlGenerator.GenerateSelectQuery(tableName, columnNamesToSelect, columnConstraints);
        }

        private IEnumerable<string> GetPropertiesInTargetType<T>()
        {
            return typeof(T).GetProperties().Select(x => x.Name);
        }

        private IList<T> ReadEntities<T>(DbCommand command) where T : new()
        {
            return _dbCommandExecutor.ExecuteQuery(command, _entityMapper.CreateInstance<T>, ConnectionString);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Async;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectInsertExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionReader _conventionReader;
        private readonly IDbCommandExecutor _dbCommandExecutor;

        public ObjectInsertExecutor(ISqlGenerator sqlGenerator, IConventionReader conventionReader, IDbCommandExecutor dbCommandExecutor)
        {
            _sqlGenerator = sqlGenerator;
            _conventionReader = conventionReader;
            _dbCommandExecutor = dbCommandExecutor;
        }

        public string ConnectionString { get; set; }

        public void Insert<T>(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();

            if (_conventionReader.HasIdentityId(typeof(T)))
            {
                var commands = CreateIdentityInsertCommands(entities);

                var ids = _dbCommandExecutor.ExecuteScalarList<int>(commands, ConnectionString);

                for (int i = 0; i < ids.Count; i++)
                {
                    _conventionReader.SetId(entityList[i], ids[i]);
                }
            }
            else
            {
                var commands = CreateInsertCommands(entities);

                _dbCommandExecutor.ExecuteNonQuery(commands, ConnectionString);
            }
        }

        public void InsertAsync<T>(IEnumerable<T> entities, Action callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(() => Insert(entities), callback, errorCallback);
        }

        private IEnumerable<DbCommand> CreateInsertCommands<TEntity>(IEnumerable<TEntity> entities)
        {
            return CreateInsertCommands(entities, (tableName, values) => _sqlGenerator.CreateInsertCommand(tableName, values));
        }
        
        private IEnumerable<ScalarCommand> CreateIdentityInsertCommands<TEntity>(IEnumerable<TEntity> entities)
        {
            return CreateInsertCommands(entities, (tableName, values) => _sqlGenerator.CreateIdentityInsertCommand(tableName, values));
        }

        private IEnumerable<TCommand> CreateInsertCommands<TEntity, TCommand>(IEnumerable<TEntity> entities, Func<string, IDictionary<string, object>, TCommand> f)
        {
            var commands = new List<TCommand>();

            foreach (var entity in entities)
            {
                var columnValues = _conventionReader.GetColumnValuesForInsertOrUpdate(entity);
                var tableName = _conventionReader.GetTableName<TEntity>();

                var command = f(tableName, columnValues);

                commands.Add(command);
            }

            return commands;
        }
    }
}
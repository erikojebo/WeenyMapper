using System;
using System.Collections.Generic;
using System.Data.Common;
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
            var commands = CreateInsertCommands(entities);

            _dbCommandExecutor.ExecuteNonQuery(commands, ConnectionString);
        }

        public void InsertAsync<T>(IEnumerable<T> entities, Action callback)
        {
            var commands = CreateInsertCommands(entities);

            TaskRunner.Run(() => _dbCommandExecutor.ExecuteNonQuery(commands, ConnectionString), callback);
        }

        private IEnumerable<DbCommand> CreateInsertCommands<T>(IEnumerable<T> entities)
        {
            var commands = new List<DbCommand>();

            foreach (var entity in entities)
            {
                var columnValues = _conventionReader.GetColumnValuesForInsert(entity);
                var tableName = _conventionReader.GetTableName<T>();
                var command = _sqlGenerator.CreateInsertCommand(tableName, columnValues);

                commands.Add(command);
            }
            return commands;
        }
    }
}
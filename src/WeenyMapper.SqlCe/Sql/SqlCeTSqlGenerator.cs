using System.Collections.Generic;

namespace WeenyMapper.Sql
{
    public class SqlCeTSqlGenerator : TSqlGenerator
    {
        private readonly IDbCommandFactory _commandFactory;

        public SqlCeTSqlGenerator(IDbCommandFactory commandFactory) : base(commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public override ScalarCommand CreateIdentityInsertCommand(string tableName, IDictionary<string, object> columnValues)
        {
            var scalarCommand = new ScalarCommand();

            var insertCommand = CreateInsertCommand(tableName, columnValues);
            var selectCommand = _commandFactory.CreateCommand("SELECT CAST(@@IDENTITY AS int)");

            scalarCommand.PreparatoryCommands.Add(insertCommand);
            scalarCommand.ResultCommand = selectCommand;

            return scalarCommand;
        }
    }
}
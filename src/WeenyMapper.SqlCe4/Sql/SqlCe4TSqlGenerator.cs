using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.SqlCe4.Sql
{
    public class SqlCe4TSqlGenerator : TSqlGenerator
    {
        private readonly IDbCommandFactory _commandFactory;

        public SqlCe4TSqlGenerator(IDbCommandFactory commandFactory) : base(commandFactory)
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
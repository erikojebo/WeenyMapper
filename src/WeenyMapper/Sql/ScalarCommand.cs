using System.Collections.Generic;
using System.Data.Common;

namespace WeenyMapper.Sql
{
    public class ScalarCommand
    {
        private readonly IDbCommandExecutor _commandExecutor;

        public ScalarCommand(IDbCommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
            PreparatoryCommands = new List<DbCommand>();
        }

        public IList<DbCommand> PreparatoryCommands { get; private set; }
        public DbCommand ResultCommand { get; set; }

        public int Execute(string connectionString)
        {
            foreach (var preparatoryCommand in PreparatoryCommands)
            {
                _commandExecutor.ExecuteNonQuery(preparatoryCommand, connectionString);
            }

            return _commandExecutor.ExecuteScalar<int>(ResultCommand, connectionString);
        }
    }
}
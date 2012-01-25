using System.Collections.Generic;
using System.Data.Common;

namespace WeenyMapper.Sql
{
    public class ScalarCommand
    {
        public ScalarCommand()
        {
            PreparatoryCommands = new List<DbCommand>();
        }

        public IList<DbCommand> PreparatoryCommands { get; private set; }
        public DbCommand ResultCommand { get; set; }
    }
}
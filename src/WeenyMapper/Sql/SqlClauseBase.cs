using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Sql
{
    public abstract class SqlClauseBase
    {
        protected SqlClauseBase()
        {
            CommandParameters = new List<CommandParameter>();
        }

        public abstract string CommandString { get; }
        protected abstract bool IsEmpty { get; }

        public IList<CommandParameter> CommandParameters { get; set; }

        public void AppendTo(DbCommand command, IDbCommandFactory commandFactory)
        {
            if (IsEmpty)
            {
                return;
            }

            command.CommandText += " " + CommandString;

            var dbParameters = CommandParameters.Select(commandFactory.CreateParameter).ToArray();
            command.Parameters.AddRange(dbParameters);
        }
    }
}
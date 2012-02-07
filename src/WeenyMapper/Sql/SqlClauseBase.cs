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

            AppendParameters(command, commandFactory);
        }

        public void InsertAtMarker(DbCommand command, string marker, IDbCommandFactory commandFactory)
        {
            InsertAtMarker(command, marker, commandFactory, "");
        }

        public void InsertWithSpaceAtMarker(DbCommand command, string marker, IDbCommandFactory commandFactory)
        {
            InsertAtMarker(command, marker, commandFactory, " ");
        }

        private void InsertAtMarker(DbCommand command, string marker, IDbCommandFactory commandFactory, string prefix)
        {
            if (IsEmpty)
            {
                command.CommandText = command.CommandText.Replace(marker, "");
                return;
            }

            command.CommandText = command.CommandText.Replace(marker, prefix + CommandString);

            AppendParameters(command, commandFactory);
        }

        private void AppendParameters(DbCommand command, IDbCommandFactory commandFactory)
        {
            var dbParameters = CommandParameters.Select(commandFactory.CreateParameter).ToArray();
            command.Parameters.AddRange(dbParameters);
        }
    }
}
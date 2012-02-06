using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class WhereClause
    {
        public WhereClause(string constraintString)
        {
            SetConstraintString(constraintString);
        }

        public string CommandString { get; set; }
        public IList<CommandParameter> CommandParameters { get; set; }

        public void AppendTo(DbCommand command, IDbCommandFactory commandFactory)
        {
            command.CommandText += CommandString;

            var dbParameters = CommandParameters.Select(commandFactory.CreateParameter).ToArray();
            command.Parameters.AddRange(dbParameters);
        }

        private void SetConstraintString(string constraintString)
        {
            if (!string.IsNullOrWhiteSpace(constraintString))
            {
                CommandString = " WHERE " + constraintString;
            }
        }
    }
}
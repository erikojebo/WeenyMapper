using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class WhereClause
    {
        private readonly string _constraintString;

        public WhereClause(string constraintString)
        {
            _constraintString = constraintString;
        }

        public string CommandString
        {
            get { return "WHERE " + _constraintString; }
        }

        protected bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(_constraintString); }
        }

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
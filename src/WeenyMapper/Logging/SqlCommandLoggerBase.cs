using System;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Logging
{
    public abstract class SqlCommandLoggerBase : ISqlCommandLogger
    {
        public void Log(DbCommand command)
        {
            var logEntry = command.CommandText;

            logEntry = AppendParameterValues(command, logEntry);

            OutputLogEntry(logEntry);
        }

        private string AppendParameterValues(DbCommand command, string logEntry)
        {
            var parameterValues = command.Parameters.OfType<DbParameter>().Select(Stringify).ToList();
            var parameterValuesString = String.Join(", ", parameterValues);

            if (!parameterValues.Any())
                return logEntry;

            return string.Format("{0} ({1})", logEntry, parameterValuesString);
        }

        protected abstract void OutputLogEntry(string logEntry);

        private string Stringify(DbParameter parameter)
        {
            return string.Format("@{0}: {1}", parameter.ParameterName, parameter.Value);
        }
    }
}
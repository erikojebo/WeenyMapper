using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Logging
{
    public abstract class SqlCommandLoggerBase : ISqlCommandLogger
    {
        public void Log(DbCommand command)
        {
            var parameterValues = command.Parameters.OfType<DbParameter>().Select(Stringify);
            var parameterValuesString = String.Join(", ", (IEnumerable<string>)parameterValues);
            var logEntry = string.Format("{0} ({1})", command.CommandText, parameterValuesString);
            OutputLogEntry(logEntry);
        }

        protected abstract void OutputLogEntry(string logEntry);

        private string Stringify(DbParameter parameter)
        {
            return string.Format("@{0}: {1}", parameter.ParameterName, parameter.Value);
        }
    }
}
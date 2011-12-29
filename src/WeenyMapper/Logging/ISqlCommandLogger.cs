using System;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Logging
{
    public interface ISqlCommandLogger
    {
        void Log(DbCommand command);
    }

    public class ConsoleSqlCommandLogger : ISqlCommandLogger
    {
        public void Log(DbCommand command)
        {
            var parameterValues = command.Parameters.OfType<DbParameter>().Select(Stringify);
            var parameterValuesString = string.Join(", ", parameterValues);
            Console.WriteLine("{0} ({1})", command.CommandText, parameterValuesString);
        }

        private string Stringify(DbParameter parameter)
        {
            return string.Format("@{0}: {1}", parameter.ParameterName, parameter.Value);
        }
    }
}
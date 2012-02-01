using System.Diagnostics;

namespace WeenyMapper.Logging
{
    public class DebugSqlCommandLogger : SqlCommandLoggerBase 
    {
        protected override void OutputLogEntry(string logEntry)
        {
            Debug.WriteLine(logEntry);
        }
    }
}
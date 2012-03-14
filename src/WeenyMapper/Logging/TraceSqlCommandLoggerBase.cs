using System.Diagnostics;

namespace WeenyMapper.Logging
{
    public class TraceSqlCommandLogger : SqlCommandLoggerBase 
    {
        protected override void OutputLogEntry(string logEntry)
        {
            Trace.WriteLine(logEntry);
        }
    }
}
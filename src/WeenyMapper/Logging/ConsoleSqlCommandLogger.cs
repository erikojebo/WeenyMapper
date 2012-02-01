using System;

namespace WeenyMapper.Logging
{
    public class ConsoleSqlCommandLogger : SqlCommandLoggerBase
    {
        protected override void OutputLogEntry(string logEntry)
        {
            Console.WriteLine(logEntry);
        }
    }
}
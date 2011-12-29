using System.Data.Common;

namespace WeenyMapper.Logging
{
    public class NullSqlCommandLogger : ISqlCommandLogger
    {
        public void Log(DbCommand command) {}
    }
}
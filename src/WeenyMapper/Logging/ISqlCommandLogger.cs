using System.Data.Common;

namespace WeenyMapper.Logging
{
    public interface ISqlCommandLogger
    {
        void Log(DbCommand command);
    }
}
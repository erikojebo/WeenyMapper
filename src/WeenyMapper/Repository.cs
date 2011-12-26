using System;

namespace WeenyMapper
{
    public class Repository
    {
        public string ConnectionString { get; set; }

        public dynamic Insert
        {
            get { return new DynamicInsertExecutor { ConnectionString = ConnectionString }; }
        }

        public dynamic Find
        {
            get { return new DynamicSelectExecutor { ConnectionString = ConnectionString }; }
        }
    }
}
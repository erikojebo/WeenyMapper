using System;
using WeenyMapper.QueryBuilding;

namespace WeenyMapper
{
    public class Repository
    {
        public string ConnectionString { get; set; }

        public dynamic Insert
        {
            get { return new DynamicInsertBuilder { ConnectionString = ConnectionString }; }
        }

        public dynamic Update
        {
            get { return new DynamicUpdateBuilder { ConnectionString = ConnectionString }; }
        }

        public dynamic Find
        {
            get { return new DynamicSelectBuilder { ConnectionString = ConnectionString }; }
        }
    }
}
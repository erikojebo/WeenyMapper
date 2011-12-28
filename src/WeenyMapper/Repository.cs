using WeenyMapper.Conventions;
using WeenyMapper.QueryBuilding;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper
{
    public class Repository
    {
        public string ConnectionString { get; set; }

        static Repository()
        {
            Convention = new DefaultConvention();
        }

        public static IConvention Convention { get; set; }

        public dynamic Insert
        {
            get
            {
                return new DynamicInsertBuilder(Convention, new TSqlGenerator(), new PropertyReader(Convention))
                    {
                        ConnectionString = ConnectionString
                    };
            }
        }

        public dynamic Update
        {
            get
            {
                return new DynamicUpdateBuilder(Convention, new TSqlGenerator())
                    {
                        ConnectionString = ConnectionString
                    };
            }
        }

        public dynamic Find
        {
            get
            {
                var objectQueryExecutor = new ObjectQueryExecutor(Convention, new TSqlGenerator());

                return new DynamicSelectBuilder(new QueryParser(), objectQueryExecutor)
                    {
                        ConnectionString = ConnectionString
                    };
            }
        }
    }
}
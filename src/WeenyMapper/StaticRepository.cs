using WeenyMapper.Conventions;
using WeenyMapper.QueryBuilding;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper
{
    public class StaticRepository
    {
        public string ConnectionString { get; set; }

        static StaticRepository()
        {
            Convention = new DefaultConvention();
        }

        public static IConvention Convention { get; set; }

        public StaticInsertBuilder<T> Insert<T>(T instance)
        {
            return new StaticInsertBuilder<T>(Convention, new TSqlGenerator())
                {
                    ConnectionString = ConnectionString
                };
        }

        public StaticUpdateBuilder<T> Update<T>(T instance)
        {
            return new StaticUpdateBuilder<T>(Convention, new TSqlGenerator())
                {
                    ConnectionString = ConnectionString
                };
        }

        public StaticSelectBuilder<T> Find<T>() where T : new()
        {
            var objectQueryExecutor = new ObjectQueryExecutor(Convention, new TSqlGenerator());

            return new StaticSelectBuilder<T>(new QueryParser(), objectQueryExecutor)
                {
                    ConnectionString = ConnectionString
                };
        }
    }
}
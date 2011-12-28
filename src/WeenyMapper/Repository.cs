using WeenyMapper.Conventions;
using WeenyMapper.QueryBuilding;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

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

        public void Insert<T>(T instance)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(Convention, new TSqlGenerator(), new PropertyReader(Convention))
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.Insert(instance);
        }

        public void Update<T>(T instance)
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(Convention, new TSqlGenerator(), new PropertyReader(Convention))
            {
                ConnectionString = ConnectionString
            };

            var builder = new DynamicUpdateBuilder<T>(objectUpdateExecutor);

            builder.Update(instance);
        }

        public dynamic DynamicFind<T>() where T : new()
        {
            var objectQueryExecutor = new ObjectQueryExecutor(Convention, new TSqlGenerator());

            return new DynamicSelectBuilder<T>(new QueryParser(), objectQueryExecutor)
                {
                    ConnectionString = ConnectionString
                };
        }

        public StaticSelectBuilder<T> Find<T>() where T : new()
        {
            var objectQueryExecutor = new ObjectQueryExecutor(Convention, new TSqlGenerator());

            return new StaticSelectBuilder<T>(objectQueryExecutor)
            {
                ConnectionString = ConnectionString
            };
        }
    }
}
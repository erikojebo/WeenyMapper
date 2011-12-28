using WeenyMapper.Conventions;
using WeenyMapper.QueryBuilding;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

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

        public void Insert<T>(T instance)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(Convention, new TSqlGenerator(),new PropertyReader(Convention))
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.Insert(instance);
        }

        public void Update<T>(T instance)
        {
            var builder = new StaticUpdateBuilder<T>(Convention, new TSqlGenerator(), new PropertyReader(Convention))
                {
                    ConnectionString = ConnectionString
                };

            builder.Update(instance);
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
using System;
using System.Collections.Generic;
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

        public void InsertMany<T>(params T[] entities)
        {
            InsertMany((IEnumerable<T>)entities);
        }

        public void InsertMany<T>(IEnumerable<T> entities)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
            {
                ConnectionString = ConnectionString
            };

            objectInsertExecutor.Insert(entities);
        }

        public void Insert<T>(T entity)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.Insert(new[] { entity });
        }

        public void Update<T>(T entity)
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            var builder = new DynamicUpdateBuilder<T>(objectUpdateExecutor);

            builder.Update(entity);
        }

        public StaticUpdateBuilder<T> Update<T>()
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return new StaticUpdateBuilder<T>(objectUpdateExecutor);
        }

        public dynamic DynamicUpdate<T>()
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return new DynamicUpdateBuilder<T>(objectUpdateExecutor);
        }

        public dynamic DynamicFind<T>() where T : new()
        {
            var objectQueryExecutor = new ObjectQueryExecutor(Convention, new TSqlGenerator(), new SqlCommandExecutor());

            return new DynamicSelectBuilder<T>(new QueryParser(), objectQueryExecutor)
                {
                    ConnectionString = ConnectionString
                };
        }

        public StaticSelectBuilder<T> Find<T>() where T : new()
        {
            var objectQueryExecutor = new ObjectQueryExecutor(Convention, new TSqlGenerator(), new SqlCommandExecutor());

            return new StaticSelectBuilder<T>(objectQueryExecutor)
                {
                    ConnectionString = ConnectionString
                };
        }

        public void Delete<T>(T entity)
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            objectDeleteExecutor.Delete(entity);
        }

        public StaticDeleteBuilder<T> Delete<T>()
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return new StaticDeleteBuilder<T>(objectDeleteExecutor);
        }

        public dynamic DynamicDelete<T>()
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return new DynamicDeleteBuilder<T>(objectDeleteExecutor);
        }

        public StaticCountBuilder<T> Count<T>()
        {
            var objectCountExecutor = new ObjectCountExecutor(new TSqlGenerator(), new ConventionDataReader(Convention), new SqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return new StaticCountBuilder<T>(objectCountExecutor);
        }
    }
}
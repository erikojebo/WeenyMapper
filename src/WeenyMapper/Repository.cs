using System;
using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.Conventions;
using WeenyMapper.Logging;
using WeenyMapper.Mapping;
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
            SqlLogger = new NullSqlCommandLogger();
        }

        public static IConvention Convention { get; set; }
        public static ISqlCommandLogger SqlLogger { get; set; }

        public void InsertMany<T>(params T[] entities)
        {
            InsertMany((IEnumerable<T>)entities);
        }

        public void InsertMany<T>(IEnumerable<T> entities)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.Insert(entities);
        }

        public void Insert<T>(T entity)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.Insert(new[] { entity });
        }

        public void InsertAsync<T>(T entity, Action callback)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.InsertAsync(new[] { entity }, callback);
        }

        public void InsertManyAsync<T>(IEnumerable<T> entities, Action callback)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.InsertAsync(entities, callback);
        }

        public int Update<T>(T entity)
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            var builder = new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());

            return builder.Update(entity);
        }

        public void UpdateAsync<T>(T entity, Action callback)
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            var builder = new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());

            builder.UpdateAsync(entity, callback);
        }

        public StaticUpdateBuilder<T> Update<T>()
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            return new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());
        }

        public StaticSelectBuilder<T> Find<T>() where T : new()
        {
            var objectQueryExecutor = new ObjectQueryExecutor(Convention, new TSqlGenerator(), new SqlCommandExecutor(SqlLogger), new EntityMapper(Convention), new ConventionReader(Convention))
                {
                    ConnectionString = ConnectionString
                };

            return new StaticSelectBuilder<T>(objectQueryExecutor, new ExpressionParser());
        }

        public int Delete<T>(T entity)
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            return objectDeleteExecutor.Delete(entity);
        }

        public void DeleteAsync<T>(T entity, Action callback)
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            objectDeleteExecutor.DeleteAsync(entity, callback);
        }

        public StaticDeleteBuilder<T> Delete<T>()
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            return new StaticDeleteBuilder<T>(objectDeleteExecutor, new ExpressionParser());
        }

        public StaticCountBuilder<T> Count<T>()
        {
            var objectCountExecutor = new ObjectCountExecutor(new TSqlGenerator(), new ConventionReader(Convention), new SqlCommandExecutor(SqlLogger))
                {
                    ConnectionString = ConnectionString
                };

            return new StaticCountBuilder<T>(objectCountExecutor, new ExpressionParser());
        }

        public void EnableSqlConsoleLogging()
        {
            SqlLogger = new ConsoleSqlCommandLogger();
        }

        public void DisableSqlConsoleLogging()
        {
            SqlLogger = new NullSqlCommandLogger();
        }

        public CustomSqlQueryExecutor<T> FindBySql<T>(DbCommand dbCommand) where T : new()
        {
            return new CustomSqlQueryExecutor<T>(new SqlCommandExecutor(SqlLogger), new EntityMapper(Convention))
                {
                    ConnectionString = ConnectionString,
                    Command = dbCommand
                };
        }
    }
}
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
        static Repository()
        {
            Convention = new DefaultConvention();
            SqlLogger = new NullSqlCommandLogger();
            DatabaseProvider = new SqlServerDatabaseProvider();
        }

        public Repository()
        {
            ConnectionString = DefaultConnectionString;
        }

        public static IConvention Convention { get; set; }
        public static ISqlCommandLogger SqlLogger { get; set; }
        public static IDatabaseProvider DatabaseProvider { get; set; }
        public static string DefaultConnectionString { get; set; }

        public string ConnectionString { get; set; }

        public void Insert<T>(params T[] entities)
        {
            InsertCollection(entities);
        }

        public void InsertCollection<T>(IEnumerable<T> entities)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.Insert(entities);
        }

        public void InsertAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.InsertAsync(new[] { entity }, callback, errorCallback);
        }

        public void InsertCollectionAsync<T>(IEnumerable<T> entities, Action callback, Action<Exception> errorCallback = null)
        {
            var objectInsertExecutor = new ObjectInsertExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            objectInsertExecutor.InsertAsync(entities, callback, errorCallback);
        }

        public int Update<T>(T entity)
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            var builder = new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());

            return builder.Update(entity);
        }

        public void UpdateAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            var builder = new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());

            builder.UpdateAsync(entity, callback, errorCallback);
        }

        public StaticUpdateBuilder<T> Update<T>()
        {
            var objectUpdateExecutor = new ObjectUpdateExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());
        }

        public StaticSelectBuilder<T> Find<T>() where T : new()
        {
            var objectQueryExecutor = new ObjectQueryExecutor(CreateSqlGenerator(), CreateSqlCommandExecutor(), new EntityMapper(CreateConventionReader()), CreateConventionReader())
                {
                    ConnectionString = ConnectionString
                };

            return new StaticSelectBuilder<T>(objectQueryExecutor, new ExpressionParser());
        }

        public int Delete<T>(T entity)
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return objectDeleteExecutor.Delete(entity);
        }

        public void DeleteAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            objectDeleteExecutor.DeleteAsync(entity, callback, errorCallback);
        }

        public StaticDeleteBuilder<T> Delete<T>()
        {
            var objectDeleteExecutor = new ObjectDeleteExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };

            return new StaticDeleteBuilder<T>(objectDeleteExecutor, new ExpressionParser());
        }

        public StaticCountBuilder<T> Count<T>()
        {
            var objectCountExecutor = new ObjectCountExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
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
            return new CustomSqlQueryExecutor<T>(CreateSqlCommandExecutor(), new EntityMapper(CreateConventionReader()))
                {
                    ConnectionString = ConnectionString,
                    Command = dbCommand
                };
        }

        private TSqlGenerator CreateSqlGenerator()
        {
            return DatabaseProvider.CreateSqlGenerator();
        }

        private DbCommandExecutor CreateSqlCommandExecutor()
        {
            return new DbCommandExecutor(SqlLogger, CreateDbCommandFactory());
        }

        private IDbCommandFactory CreateDbCommandFactory()
        {
            return DatabaseProvider.CreateDbCommandFactory();
        }

        private ConventionReader CreateConventionReader()
        {
            return new ConventionReader(Convention);
        }
    }
}
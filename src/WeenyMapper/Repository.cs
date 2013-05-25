using System;
using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.Conventions;
using WeenyMapper.Exceptions;
using WeenyMapper.Logging;
using WeenyMapper.Mapping;
using WeenyMapper.QueryBuilding;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;
using WeenyMapper.Extensions;

namespace WeenyMapper
{
    public class Repository
    {
        private string _connectionString;
        private EntityMapper _entityMapper;
        private IConvention _convention;
        private IDbCommandFactory _dbCommandFactory;

        static Repository()
        {
            DefaultConvention = new DefaultConvention();
            SqlLogger = new NullSqlCommandLogger();
            DatabaseProvider = new SqlServerDatabaseProvider();
        }

        public Repository()
        {
            ConnectionString = DefaultConnectionString;
            IsEntityCachingEnabled = IsEntityCachingEnabledByDefault;
        }

        public static IConvention DefaultConvention { get; set; }
        public static ISqlCommandLogger SqlLogger { get; set; }
        public static IDatabaseProvider DatabaseProvider { get; set; }
        public static string DefaultConnectionString { get; set; }
        public static bool IsEntityCachingEnabledByDefault { get; set; }
        public bool IsEntityCachingEnabled { get; set; }

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    var message =
                        "The connection string has not been set. " +
                        "Please set the ConnectionString property on the repository instance " +
                        "or set the static Repository.DefaultConnectionString property " +
                        "to set the connection string for all repositories created from then on.";

                    throw new WeenyMapperException(message);
                }

                return _connectionString;
            }
            set { _connectionString = value; }
        }

        public IConvention Convention
        {
            get { return _convention ?? DefaultConvention; }
            set { _convention = value; }
        }

        public void Insert<T>(params T[] entities)
        {
            var isListItemTypeCollectionInsteadOfEntity = typeof(T).ImplementsGenericInterface(typeof(IEnumerable<>));

            if (isListItemTypeCollectionInsteadOfEntity)
            {
                InsertAllCollections(entities);
            }
            else
            {
                InsertCollection(entities);    
            }
        }

        private void InsertAllCollections<T>(IEnumerable<T> entities)
        {
            foreach (var collection in entities)
            {
                dynamic self = this;
                self.InsertCollection(collection);
            }
        }

        public void InsertCollection<T>(IEnumerable<T> entities)
        {
            var objectInsertExecutor = CreateObjectInsertExecutor<T>();

            objectInsertExecutor.Insert(entities);
        }

        public void InsertAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            var objectInsertExecutor = CreateObjectInsertExecutor<T>();

            objectInsertExecutor.InsertAsync(new[] { entity }, callback, errorCallback);
        }

        public void InsertCollectionAsync<T>(IEnumerable<T> entities, Action callback, Action<Exception> errorCallback = null)
        {
            var objectInsertExecutor = CreateObjectInsertExecutor<T>();

            objectInsertExecutor.InsertAsync(entities, callback, errorCallback);
        }

        public void Update<T>(T entity)
        {
            var objectUpdateExecutor = CreateObjectUpdateExecutor<T>();

            var builder = new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());

            builder.Update(entity);
        }

        public void UpdateAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            var objectUpdateExecutor = CreateObjectUpdateExecutor<T>();
            var builder = new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());

            builder.UpdateAsync(entity, callback, errorCallback);
        }

        public StaticUpdateBuilder<T> Update<T>()
        {
            var objectUpdateExecutor = CreateObjectUpdateExecutor<T>();

            return new StaticUpdateBuilder<T>(objectUpdateExecutor, new ExpressionParser());
        }

        public StaticSelectBuilder<T> Find<T>(string startingTableAlias = null, string primaryAlias = null) where T : new()
        {
            var objectQueryExecutor = CreateSqlQueryExecutor<T>();

            var staticSelectBuilder = new StaticSelectBuilder<T>(objectQueryExecutor, new ExpressionParser(), CreateConventionReader());

            staticSelectBuilder.PrimaryAlias = primaryAlias;
            staticSelectBuilder.StartingTableAlias = startingTableAlias;

            return staticSelectBuilder;
        }

        public void Delete<T>(T entity)
        {
            var objectDeleteExecutor = CreateObjectDeleteExecutor<T>();

            objectDeleteExecutor.Delete(entity);
        }

        public void DeleteAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            var objectDeleteExecutor = CreateObjectDeleteExecutor<T>();

            objectDeleteExecutor.DeleteAsync(entity, callback, errorCallback);
        }

        public StaticDeleteBuilder<T> Delete<T>()
        {
            var objectDeleteExecutor = CreateObjectDeleteExecutor<T>();

            return new StaticDeleteBuilder<T>(objectDeleteExecutor, new ExpressionParser());
        }

        public StaticCountBuilder<T> Count<T>()
        {
            var objectCountExecutor = CreateObjectCountExecutor<T>();

            return new StaticCountBuilder<T>(objectCountExecutor, new ExpressionParser());
        }

        public static void EnableSqlConsoleLogging()
        {
            SqlLogger = new ConsoleSqlCommandLogger();
        }

        public static void EnableSqlTraceLogging()
        {
            SqlLogger = new TraceSqlCommandLogger();
        }

        public static void DisableSqlLogging()
        {
            SqlLogger = new NullSqlCommandLogger();
        }

        public virtual CustomSqlQueryExecutor<T> FindBySql<T>(DbCommand dbCommand) where T : new()
        {
            return new CustomSqlQueryExecutor<T>(CreateSqlCommandExecutor(), new EntityMapper(CreateConventionReader()))
                {
                    ConnectionString = ConnectionString,
                    Command = dbCommand
                };
        }

        protected virtual IObjectInsertExecutor CreateObjectInsertExecutor<T>()
        {
            return new ObjectInsertExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };
        }

        protected virtual IObjectUpdateExecutor CreateObjectUpdateExecutor<T>()
        {
            return new ObjectUpdateExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };
        }

        protected virtual ISqlQueryExecutor CreateSqlQueryExecutor<T>() where T : new()
        {
            return new SqlQueryExecutor(CreateSqlGenerator(), CreateSqlCommandExecutor(), CreateEntityMapper())
                {
                    ConnectionString = ConnectionString
                };
        }

        protected virtual IObjectDeleteExecutor CreateObjectDeleteExecutor<T>()
        {
            return new ObjectDeleteExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };
        }

        protected virtual IObjectCountExecutor CreateObjectCountExecutor<T>()
        {
            return new ObjectCountExecutor(CreateSqlGenerator(), CreateConventionReader(), CreateSqlCommandExecutor())
                {
                    ConnectionString = ConnectionString
                };
        }

        private TSqlGenerator CreateSqlGenerator()
        {
            return DatabaseProvider.CreateSqlGenerator();
        }

        private DbCommandExecutor CreateSqlCommandExecutor()
        {
            return new DbCommandExecutor(SqlLogger, DbCommandFactory);
        }

        private IDbCommandFactory DbCommandFactory
        {
            get
            {
                if (_dbCommandFactory == null)
                    _dbCommandFactory = DatabaseProvider.CreateDbCommandFactory();

                return _dbCommandFactory;
            }
        }

        protected EntityMapper CreateEntityMapper()
        {
            if (_entityMapper == null)
            {
                _entityMapper = new EntityMapper(CreateConventionReader())
                    {
                        IsEntityCachingEnabled = IsEntityCachingEnabled
                    };
            }

            return _entityMapper;
        }

        protected ConventionReader CreateConventionReader()
        {
            return new ConventionReader(Convention);
        }

        public ConnectionScope BeginConnection()
        {
            return DbCommandFactory.BeginConnection(ConnectionString);
        }
    }
}
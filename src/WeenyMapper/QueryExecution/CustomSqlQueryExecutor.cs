using System;
using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.Mapping;
using WeenyMapper.Sql;
using System.Linq;

namespace WeenyMapper.QueryExecution
{
    public class CustomSqlQueryExecutor<T> where T : new()
    {
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private readonly IEntityMapper _entityMapper;

        public CustomSqlQueryExecutor(IDbCommandExecutor dbCommandExecutor, IEntityMapper entityMapper)
        {
            _dbCommandExecutor = dbCommandExecutor;
            _entityMapper = entityMapper;
        }

        public T Execute()
        {
            return ExecuteList().First();
        }
        
        public IList<T> ExecuteList()
        {
            return _dbCommandExecutor.ExecuteQuery(Command, _entityMapper.CreateInstance<T>, ConnectionString);
        }

        public DbCommand Command { get; set; }
        public string ConnectionString { get; set; }

        public TScalar ExecuteScalar<TScalar>()
        {
            return _dbCommandExecutor.ExecuteScalar<TScalar>(Command, ConnectionString);
        }

        public IList<TScalar> ExecuteScalarList<TScalar>()
        {
            return _dbCommandExecutor.ExecuteScalarList<TScalar>(Command, ConnectionString);
        }
    }
}
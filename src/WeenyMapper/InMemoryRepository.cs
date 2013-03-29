using System;
using System.Collections.Generic;
using WeenyMapper.Exceptions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Sql;

namespace WeenyMapper
{
    public class InMemoryRepository : Repository
    {
        public override CustomSqlQueryExecutor<T> FindBySql<T>(System.Data.Common.DbCommand dbCommand)
        {
            throw new WeenyMapperException("The InMemoryRepository does not support custom SQL queries");
        }

        protected override IObjectQueryExecutor CreateObjectQueryExecutor<T>()
        {
            return new InMemoryObjectQueryExecutor();
        }

        protected override IObjectCountExecutor CreateObjectCountExecutor<T>()
        {
            return base.CreateObjectCountExecutor<T>();
        }

        protected override IObjectDeleteExecutor CreateObjectDeleteExecutor<T>()
        {
            return base.CreateObjectDeleteExecutor<T>();
        }

        protected override IObjectInsertExecutor CreateObjectInsertExecutor<T>()
        {
            return base.CreateObjectInsertExecutor<T>();
        }

        protected override IObjectUpdateExecutor CreateObjectUpdateExecutor<T>()
        {
            return base.CreateObjectUpdateExecutor<T>();
        }
    }

    public class InMemoryObjectQueryExecutor : IObjectQueryExecutor
    {
        public string ConnectionString { get; set; }
        public IList<T> Find<T>(ObjectQuerySpecification querySpecification) where T : new()
        {
            throw new NotImplementedException();
        }

        public TScalar FindScalar<T, TScalar>(ObjectQuerySpecification querySpecification)
        {
            throw new NotImplementedException();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuerySpecification querySpecification)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticSelectBuilder<T> : StaticCommandBuilderBase<T> where T : new()
    {
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private IDictionary<string, object> _constraints = new Dictionary<string, object>();

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor)
        {
            _objectQueryExecutor = objectQueryExecutor;
        }

        public StaticSelectBuilder<T> By<TReturnValue>(Expression<Func<T, TReturnValue>> getter, TReturnValue value)
        {
            StorePropertyValue(getter, value, _constraints);
            return this;
        }

        public T Execute()
        {
            return ExecuteList().First();
        }

        public IList<T> ExecuteList()
        {
            return _objectQueryExecutor.Find<T>(typeof(T).Name, _constraints);
        }
    }
}
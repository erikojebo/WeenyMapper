using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticDeleteBuilder<T> : StaticCommandBuilderBase<T>
    {
        private readonly ObjectDeleteExecutor _objectDeleteExecutor;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();

        public StaticDeleteBuilder(ObjectDeleteExecutor objectDeleteExecutor)
        {
            _objectDeleteExecutor = objectDeleteExecutor;
        }

        public StaticDeleteBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _constraints);
            return this;
        }

        public void Execute()
        {
            _objectDeleteExecutor.Delete<T>(_constraints);
        }
    }
}
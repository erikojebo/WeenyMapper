using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class StaticCountBuilder<T> : StaticCommandBuilderBase<T>
    {
        private readonly ObjectCountExecutor _objectCountExecutor;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();

        public StaticCountBuilder(ObjectCountExecutor objectCountExecutor)
        {
            _objectCountExecutor = objectCountExecutor;
        }

        public StaticCountBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _constraints);
            return this;
        }

        public int Execute()
        {
            return _objectCountExecutor.Count<T>(_constraints);
        }
    }
}
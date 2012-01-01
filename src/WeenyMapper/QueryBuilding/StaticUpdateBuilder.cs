using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.Async;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticUpdateBuilder<T> : StaticCommandBuilderBase<T>
    {
        private readonly IObjectUpdateExecutor _objectUpdateExecutor;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _setters = new Dictionary<string, object>();

        public StaticUpdateBuilder(IObjectUpdateExecutor objectUpdateExecutor)
        {
            _objectUpdateExecutor = objectUpdateExecutor;
        }

        public int Update(T instance)
        {
            return _objectUpdateExecutor.Update(instance);
        }

        public StaticUpdateBuilder<T> Where<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _constraints);
            return this;
        }

        public StaticUpdateBuilder<T> Set<TValue>(Expression<Func<T, TValue>> getter, TValue value)
        {
            StorePropertyValue(getter, value, _setters);
            return this;
        }

        public int Execute()
        {
            return _objectUpdateExecutor.Update<T>(_constraints, _setters);
        }

        public void UpdateAsync(T entity, Action callback)
        {
            TaskRunner.Run(() => Update(entity), callback);
        }
    }
}
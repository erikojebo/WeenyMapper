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
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private readonly IList<string> _propertiesToSelect = new List<string>();

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor)
        {
            _objectQueryExecutor = objectQueryExecutor;
        }

        public StaticSelectBuilder<T> Where<TReturnValue>(Expression<Func<T, TReturnValue>> getter, TReturnValue value)
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
            if (_propertiesToSelect.Any())
            {
                return _objectQueryExecutor.Find<T>(typeof(T).Name, _constraints, _propertiesToSelect);
            }
            return _objectQueryExecutor.Find<T>(typeof(T).Name, _constraints);
        }

        public StaticSelectBuilder<T> Select<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);
            _propertiesToSelect.Add(propertyName);
            return this;
        }
    }
}
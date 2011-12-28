using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryBuilding
{
    public class StaticSelectBuilder<T> where T : new()
    {
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private IDictionary<string, object> _constraints = new Dictionary<string, object>();

        public StaticSelectBuilder(IObjectQueryExecutor objectQueryExecutor)
        {
            _objectQueryExecutor = objectQueryExecutor;
        }

        public string ConnectionString
        {
            get { return _objectQueryExecutor.ConnectionString; }
            set { _objectQueryExecutor.ConnectionString = value; }
        }

        public StaticSelectBuilder<T> By<TReturnValue>(Expression<Func<T, TReturnValue>> getter, TReturnValue value)
        {
            var propertyName = PropertyMetadataReader<T>.GetPropertyName(getter);
            _constraints[propertyName] = value;
            return this;
        }

        public T Execute()
        {
            return _objectQueryExecutor.Find<T>(typeof(T).Name, _constraints);
        }
    }
}
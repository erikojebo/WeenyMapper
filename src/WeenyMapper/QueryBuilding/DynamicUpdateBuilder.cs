using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.QueryExecution;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicUpdateBuilder<T> : DynamicObject
    {
        private readonly IObjectUpdateExecutor _objectUpdateExecutor;

        public DynamicUpdateBuilder(IObjectUpdateExecutor objectUpdateExecutor)
        {
            _objectUpdateExecutor = objectUpdateExecutor;
        }

        public void Update(T instance)
        {
            _objectUpdateExecutor.Update(instance);
        }
    }
}
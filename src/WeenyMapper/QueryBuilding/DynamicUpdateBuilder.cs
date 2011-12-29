using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicUpdateBuilder<T> : DynamicCommandBuilderBase
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

        protected override IEnumerable<string> ValidPrefixes
        {
            get { return new[] { "Where", "Set" }; }
        }

        public void Execute()
        {
            var constraints = GetPropertyValues("Where");
            var setters = GetPropertyValues("Set");

            _objectUpdateExecutor.Update<T>(constraints, setters);
        }
    }
}
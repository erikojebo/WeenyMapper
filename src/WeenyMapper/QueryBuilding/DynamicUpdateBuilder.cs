using System;
using System.Collections.Generic;
using WeenyMapper.Async;
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

        public int Update(T instance)
        {
            return _objectUpdateExecutor.Update(instance);
        }

        protected override IEnumerable<MethodPatternDescription> MethodPatternDescriptions
        {
            get
            {
                return new[]
                    {
                        new MethodPatternDescription { HasParameter = true, MethodNamePrefix = "Where" },
                        new MethodPatternDescription { HasParameter = true, MethodNamePrefix = "Set" },
                    };
            }
        }

        public int Execute()
        {
            var constraints = GetPropertyValues("Where");
            var setters = GetPropertyValues("Set");

            return _objectUpdateExecutor.Update<T>(constraints, setters);
        }

        public void ExecuteAsync(Action<int> callback)
        {
            TaskRunner.Run(Execute, callback);
        }
    }
}
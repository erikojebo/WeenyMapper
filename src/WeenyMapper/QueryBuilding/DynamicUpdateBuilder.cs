using System.Collections.Generic;
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

        protected override IEnumerable<string> ValidPrefixes
        {
            get { return new[] { "Where", "Set" }; }
        }

        public int Execute()
        {
            var constraints = GetPropertyValues("Where");
            var setters = GetPropertyValues("Set");

            return _objectUpdateExecutor.Update<T>(constraints, setters);
        }
    }
}
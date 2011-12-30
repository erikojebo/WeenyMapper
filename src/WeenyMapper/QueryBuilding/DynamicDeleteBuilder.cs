using System.Collections.Generic;
using WeenyMapper.Extensions;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicDeleteBuilder<T> : DynamicCommandBuilderBase
    {
        private readonly IObjectDeleteExecutor _objectDeleteExecutor;

        public DynamicDeleteBuilder(IObjectDeleteExecutor objectDeleteExecutor)
        {
            _objectDeleteExecutor = objectDeleteExecutor;
        }

        public int Execute()
        {
            var constraints = GetPropertyValues("Where");
            return _objectDeleteExecutor.Delete<T>(constraints);
        }

        protected override IEnumerable<MethodPatternDescription> MethodPatternDescriptions
        {
            get
            {
                return new[]
                    {
                        new MethodPatternDescription { HasParameter = true, MethodNamePrefix = "Where" },
                    };

            }
        }
    }
}
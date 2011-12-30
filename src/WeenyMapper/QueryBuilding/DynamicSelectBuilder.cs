using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Exceptions;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicSelectBuilder<T> : DynamicCommandBuilderBase where T : new()
    {
        private readonly IObjectQueryExecutor _objectQueryExecutor;

        public DynamicSelectBuilder(IObjectQueryExecutor objectQueryExecutor)
        {
            _objectQueryExecutor = objectQueryExecutor;
        }

        public T Execute()
        {
            var result = ExecuteList();

            if (!result.Any())
            {
                throw new WeenyMapperException("No rows matched the given query");
            }

            return result.First();
        }

        public IList<T> ExecuteList()
        {
            var constraints = GetPropertyValues("Where");
            var propertiesToSelect = GetPropertyNames("Select");

            if (propertiesToSelect.Any())
            {
                return _objectQueryExecutor.Find<T>(typeof(T).Name, constraints, propertiesToSelect);
            }

            return _objectQueryExecutor.Find<T>(typeof(T).Name, constraints);
        }

        protected override IEnumerable<MethodPatternDescription> MethodPatternDescriptions
        {
            get
            {
                return new[]
                    {
                        new MethodPatternDescription { HasParameter = true, MethodNamePrefix = "Where" },
                        new MethodPatternDescription { HasParameter = false, MethodNamePrefix = "Select" },
                    };
            }
        }
    }

    public class MethodPatternDescription
    {
        public bool HasParameter { get; set; }
        public string MethodNamePrefix { get; set; }
    }
}
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Extensions;
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
            return ExecuteList().First();
        }

        public IList<T> ExecuteList()
        {
            var constraints = GetPropertyValues("By");
            return _objectQueryExecutor.Find<T>(typeof(T).Name, constraints);
        }

        protected override IEnumerable<string> ValidPrefixes
        {
            get { return "By".AsList(); }
        }
    }
}
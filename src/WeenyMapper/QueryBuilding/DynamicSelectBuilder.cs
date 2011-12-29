using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicSelectBuilder<T> : DynamicObject where T : new()
    {
        private readonly IQueryParser _queryParser;
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();

        public DynamicSelectBuilder(IQueryParser queryParser, IObjectQueryExecutor objectQueryExecutor)
        {
            _queryParser = queryParser;
            _objectQueryExecutor = objectQueryExecutor;
        }

        public string ConnectionString
        {
            get { return _objectQueryExecutor.ConnectionString; }
            set { _objectQueryExecutor.ConnectionString = value; }
        }

        public T Execute()
        {
            return ExecuteList().First();
        }

        public IList<T> ExecuteList()
        {
            return _objectQueryExecutor.Find<T>(typeof(T).Name, _constraints);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var constraintProperties = _queryParser.GetConstraintProperties(binder.Name);

            for (int i = 0; i < constraintProperties.Count; i++)
            {
                var propertyName = constraintProperties[i];
                var propertyValue = args[i];

                _constraints[propertyName] = propertyValue;
            }

            result = this;

            return true;
        }
    }
}
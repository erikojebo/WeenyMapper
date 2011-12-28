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
        private List<string> _constraintProperties = new List<string>();
        private List<object> _constraintValues = new List<object>();

        public DynamicSelectBuilder() : this(new QueryParser(), new ObjectQueryExecutor()) {}

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
            var constraints = new Dictionary<string, object>();

            for (int i = 0; i < _constraintProperties.Count; i++)
            {
                constraints[_constraintProperties[i]] = _constraintValues[i];
            }

            return _objectQueryExecutor.Find<T>(typeof(T).Name, constraints);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var constraintProperties = _queryParser.GetConstraintProperties(binder.Name);

            _constraintProperties.AddRange(constraintProperties);
            _constraintValues.AddRange(args.ToList());

            result = this;

            return true;
        }
    }
}
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicSelectBuilder : DynamicObject
    {
        private readonly IQueryParser _queryParser;
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private string _className;
        private SelectQuery _query;
        private List<object> _constraintValues;

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

        public T Execute<T>() where T : new()
        {
            var constraints = new Dictionary<string, object>();

            for (int i = 0; i < _query.ConstraintProperties.Count; i++)
            {
                constraints[_query.ConstraintProperties[i]] = _constraintValues[i];
            }

            return _objectQueryExecutor.Find<T>(_className, constraints);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            _query = _queryParser.ParseSelectQuery(binder.Name);
            _constraintValues = args.ToList();
            _className = _query.ClassName;

            result = this;

            return true;
        }
    }
}
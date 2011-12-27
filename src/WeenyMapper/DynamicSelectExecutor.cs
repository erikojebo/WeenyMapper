using System.Collections.Generic;
using System.Dynamic;
using WeenyMapper.QueryParsing;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper
{
    public class DynamicSelectExecutor : DynamicObject
    {
        private readonly IQueryParser _queryParser;
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private string _className;
        private SelectQuery _query;
        private List<object> _constraintValues;

        public DynamicSelectExecutor() : this(new QueryParser(), new ObjectQueryExecutor()) {}

        public DynamicSelectExecutor(IQueryParser queryParser, IObjectQueryExecutor objectQueryExecutor)
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
            var columnName = _query.ConstraintProperties[0];
            constraints[columnName] = _constraintValues[0];

            return _objectQueryExecutor.Find<T>(_className, constraints);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            _query = _queryParser.ParseSelectQuery(binder.Name);
            _constraintValues = new List<object> { args[0] };
            _className = _query.ClassName;

            result = this;

            return true;
        }
    }
}
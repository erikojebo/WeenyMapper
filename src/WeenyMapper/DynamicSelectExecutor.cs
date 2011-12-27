using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using WeenyMapper.Conventions;
using WeenyMapper.QueryParsing;
using WeenyMapper.SqlGeneration;
using System.Linq;

namespace WeenyMapper
{
    public class DynamicSelectExecutor : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IQueryParser _queryParser;
        private readonly IObjectQueryExecutor _objectQueryExecutor;
        private string _className;
        private SelectQuery _query;
        private List<object> _constraintValues;

        public DynamicSelectExecutor() : this(new DefaultConvention(), new TSqlGenerator(), new QueryParser(), new ObjectQueryExecutor()) {}

        public DynamicSelectExecutor(IConvention convention, ISqlGenerator sqlGenerator, IQueryParser queryParser, IObjectQueryExecutor objectQueryExecutor)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
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
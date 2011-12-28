using System;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryBuilding
{
    public class StaticSelectBuilder<T> where T : new()
    {
        private readonly IQueryParser _queryParser;
        private readonly IObjectQueryExecutor _objectQueryExecutor;

        public StaticSelectBuilder(IQueryParser queryParser, IObjectQueryExecutor objectQueryExecutor)
        {
            _queryParser = queryParser;
            _objectQueryExecutor = objectQueryExecutor;
        }

        public string ConnectionString { get; set; }

        public StaticSelectBuilder<T> By<TReturnValue>(Func<T, TReturnValue> getter, TReturnValue id)
        {
            throw new NotImplementedException();
        }

        public T Execute()
        {
            throw new NotImplementedException();
        }
    }
}
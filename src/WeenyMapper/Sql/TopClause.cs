using System;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class TopClause : SqlClauseBase 
    {
        private readonly int _rowCountLimit;

        public TopClause(int rowCountLimit, ICommandParameterFactory commandParameterFactory)
        {
            _rowCountLimit = rowCountLimit;

            var parameter = commandParameterFactory.Create("RowCountLimit", _rowCountLimit);

            CommandParameters.Add(parameter);
        }

        public override string CommandString
        {
            get { return string.Format("TOP({0})", CommandParameters.First().ReferenceName); }
        }

        protected override bool IsEmpty
        {
            get { return _rowCountLimit <= 0; }
        }
    }
}
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class WhereClause : SqlClauseBase
    {
        private readonly string _constraintString;

        public WhereClause(string constraintString)
        {
            _constraintString = constraintString;
        }

        public override string CommandString
        {
            get { return "WHERE " + _constraintString; }
        }

        protected override bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(_constraintString); }
        }
    }
}
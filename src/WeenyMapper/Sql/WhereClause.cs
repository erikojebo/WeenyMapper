using WeenyMapper.QueryParsing;
using WeenyMapper.Extensions;

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

        public WhereClause Combine(WhereClause whereClause, QueryCombinationOperation combinationOperation)
        {
            var combinedConstraintString = combinationOperation.Combine(_constraintString, whereClause._constraintString);
            var combineWhereClause = new WhereClause(combinedConstraintString);

            combineWhereClause.CommandParameters.AddRange(CommandParameters);
            combineWhereClause.CommandParameters.AddRange(whereClause.CommandParameters);

            return combineWhereClause;
        }

        public static WhereClause CreateEmpty()
        {
            return new WhereClause("");
        }
    }
}
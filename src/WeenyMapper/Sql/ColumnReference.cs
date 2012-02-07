using System;

namespace WeenyMapper.Sql
{
    public class ColumnReference
    {
        private readonly string _columnName;
        private readonly string _tableName;
        private readonly Func<string, string> _escape;

        public ColumnReference(string columnName, Func<string, string> escape) : this(columnName, null, escape) {}

        public ColumnReference(string columnName, string tableName, Func<string, string> escape)
        {
            _tableName = tableName;
            _columnName = columnName;
            _escape = escape;
        }

        public override string ToString()
        {
            var columnNameString = _escape(_columnName);

            if (!string.IsNullOrWhiteSpace(_tableName))
            {
                columnNameString = _escape(_tableName) + "." + columnNameString;
            }

            return columnNameString;
        }
    }
}
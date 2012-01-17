using System;

namespace WeenyMapper.Sql
{
    public class CommandParameter
    {
        private readonly string _columnName;

        public CommandParameter(string columnName, object value)
        {
            _columnName = columnName;
            Value = value;
        }

        public string Name
        {
            get
            {
                string occurrenceNumber = ColumnNameOccurrenceIndex > 0 ? (ColumnNameOccurrenceIndex + 1).ToString() : "";
                return _columnName + "Constraint" + occurrenceNumber;
            }
        }

        public object Value { get; private set; }
        public int ColumnNameOccurrenceIndex { get; set; }

        public string ReferenceName
        {
            get { return "@" + Name; }
        }

        public string ToConstraintString(string operatorString, Func<string, string> escapeFunction)
        {
            var escapedColumnName = escapeFunction(_columnName);

            return string.Format("{0} {1} {2}", escapedColumnName, operatorString, ReferenceName);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CommandParameter;

            if (other == null)
            {
                return false;
            }

            return _columnName == other._columnName &&
                   Name == other.Name &&
                   Equals(Value, other.Value) &&
                   ColumnNameOccurrenceIndex == other.ColumnNameOccurrenceIndex;
        }

        public override string ToString()
        {
            return string.Format("({0} : {1}) #{2}", Name, Value, ColumnNameOccurrenceIndex);
        }
    }
}
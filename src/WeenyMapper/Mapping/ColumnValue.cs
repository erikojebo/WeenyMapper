using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.Mapping
{
    public class ColumnValue
    {
        private const string Separator = " ";

        public ColumnValue(string alias, object value)
        {
            Alias = alias;
            Value = value;
        }

        public string Alias { get; private set; }
        public object Value { get; private set; }

        public string ColumnName
        {
            get { return Alias.Substring(Alias.IndexOf(Separator) + 1); }
        }

        public bool HasTableQualifiedAlias
        {
            get { return Alias.Contains(Separator); }
        }

        public bool IsForType(Type type, IConvention convention)
        {
            return !HasTableQualifiedAlias || Alias.StartsWith(convention.GetTableName(type));
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Alias, Value);
        }
    }
}
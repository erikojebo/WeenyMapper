using System;
using WeenyMapper.Conventions;

namespace WeenyMapper.Mapping
{
    public class ColumnValue
    {
        private readonly object _value;
        private const string Separator = " ";

        public ColumnValue(string alias, object value)
        {
            Alias = alias;
            _value = value;
        }

        public string Alias { get; private set; }

        public object Value
        {
            get { return _value == DBNull.Value ? null : _value; }
        }

        public string ColumnName
        {
            get { return Alias.Substring(Alias.IndexOf(Separator) + 1); }
        }
        
        public string TableName
        {
            get { return Alias.Substring(0, Alias.IndexOf(Separator)); }
        }

        public bool HasTableQualifiedAlias
        {
            get { return Alias.Contains(Separator); }
        }

        public bool IsForType(Type type, IConvention convention)
        {
            return !HasTableQualifiedAlias || TableName == convention.GetTableName(type);
        }

        public bool IsForAlias(string alias)
        {
            return !HasTableQualifiedAlias || TableName == alias;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Alias, Value);
        }
    }
}
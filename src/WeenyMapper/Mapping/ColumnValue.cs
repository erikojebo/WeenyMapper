﻿using System;
using System.Collections.Generic;
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
        
        public ColumnValue(string tableIdentifier, string columnName, object value)
        {
            Alias = tableIdentifier + Separator + columnName;
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

        public bool IsForIdentifier(string tableIdentifier)
        {
            return !HasTableQualifiedAlias || TableName == tableIdentifier;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Alias, Value);
        }

        public bool MatchesValue(object value)
        {
            return Equals(Value, value);
        }

        public ColumnValue Copy(object newValue)
        {
            return new ColumnValue(Alias, newValue);
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ColumnValue;

            if (other == null)
                return false;

            return other.Alias == Alias &&
                   other.Value == Value;
        }

        public ColumnValue Clone()
        {
            return new ColumnValue(Alias, Value);
        }
    }
}
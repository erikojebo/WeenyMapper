﻿using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Conventions;

namespace WeenyMapper.Mapping
{
    public class Row
    {
        public Row()
        {
            ColumnValues = new List<ColumnValue>();
        }

        public Row(IEnumerable<KeyValuePair<string, object>> values)
        {
            ColumnValues = values.Select(x => new ColumnValue(x.Key, x.Value)).ToList();
        }

        public Row(IEnumerable<ColumnValue> columnValues)
        {
            ColumnValues = columnValues.ToList();
        }

        public IList<ColumnValue> ColumnValues { get; set; }

        public void Add(string columnAlias, object value)
        {
            ColumnValues.Add(new ColumnValue(columnAlias, value));
        }

        public bool HasValuesForType(Type type, IConvention convention)
        {
            return ColumnValues.Any(x => x.IsForType(type, convention));
        }

        public bool HasValuesForTable(string tableIdentifier)
        {
            return ColumnValues.Any(x => x.IsForIdentifier(tableIdentifier));
        }

        public IList<ColumnValue> GetColumnValuesForType(Type type, IConvention convention)
        {
            return ColumnValues.Where(x => x.IsForType(type, convention) && !IsGeneratedByWeenyMapper(x)).ToList();
        }

        public IList<ColumnValue> GetColumnValuesForAlias(string alias)
        {
            return ColumnValues.Where(x => x.IsForIdentifier(alias) && !IsGeneratedByWeenyMapper(x)).ToList();
        }

        private static bool IsGeneratedByWeenyMapper(ColumnValue x)
        {
            return x.ColumnName.StartsWith(EntityMapper.WeenyMapperGeneratedColumnNamePrefix);
        }

        public ColumnValue GetColumnValue(string columnName)
        {
            return ColumnValues.FirstOrDefault(x => x.ColumnName == columnName);
        }

        public void Remove(IEnumerable<ColumnValue> columnValues)
        {
            foreach (var columnValue in columnValues)
            {
                Remove(columnValue);
            }
        }

        public void Remove(ColumnValue columnValue)
        {
            ColumnValues.Remove(columnValue); 
        }

        public void Add(ColumnValue columnValue)
        {
            ColumnValues.Add(columnValue);
        }

        public override string ToString()
        {
            var columnStrings = ColumnValues.Select(x => x.ToString());
            return string.Join(", ", columnStrings);
        }

        public ColumnValue GetColumnValue(string tableIdentifier, string columnName)
        {
            return GetColumnValuesForAlias(tableIdentifier).FirstOrDefault(x => x.ColumnName == columnName);
        }

        public bool HasColumnValue(string tableName, string columnName)
        {
            return GetColumnValue(tableName, columnName) != null;
        }

        public void Add(IEnumerable<ColumnValue> columnValues)
        {
            foreach (var columnValue in columnValues)
            {
                Add(columnValue);
            }
        }

        public Row Clone()
        {
            return new Row(ColumnValues.Select(x => x.Clone()));
        }
    }
}
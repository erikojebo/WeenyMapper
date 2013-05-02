using System;
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

        public IList<ColumnValue> GetColumnValuesForType(Type type, IConvention convention)
        {
            return ColumnValues.Where(x => x.IsForType(type, convention) && !x.ColumnName.StartsWith(EntityMapper.WeenyMapperGeneratedColumnNamePrefix)).ToList();
        }
    }
}
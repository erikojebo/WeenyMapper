using System.Collections.Generic;
using System.Linq;

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
    }
}
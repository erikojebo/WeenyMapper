using System.Collections.Generic;

namespace WeenyMapper.Mapping
{
    public class Row
    {
        public Row()
        {
            ColumnValues = new List<ColumnValue>();
        }

        public IList<ColumnValue> ColumnValues { get; set; }

        public void Add(string columnAlias, object value)
        {
            ColumnValues.Add(new ColumnValue(columnAlias, value));
        }
    }
}
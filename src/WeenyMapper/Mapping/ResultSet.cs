using System;
using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.Mapping
{
    public class ResultSet
    {
        public ResultSet() : this(new List<Row>())
        {
        }

        public ResultSet(IEnumerable<Row> rows)
        {
            Rows = rows.ToList();
        }

        public IList<Row> Rows { get; set; }

        public void AddRow(params ColumnValue[] columnValues)
        {
            AddRow((IEnumerable<ColumnValue>)columnValues);
        }

        public void AddRow(Row row)
        {
            AddRow(row.ColumnValues);
        }

        public void AddRow(IEnumerable<ColumnValue> columnValues)
        {
            var row = new Row(columnValues);
            Rows.Add(row);
        }

        public void Remove(Row row)
        {
            Rows.Remove(row);
        }

        public void Remove(IEnumerable<Row> rows)
        {
            foreach (var row in rows)
            {
                Remove(row);
            }
        }
    }
}
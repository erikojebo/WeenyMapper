using System;
using System.Collections.Generic;

namespace WeenyMapper.Mapping
{
    public class ResultSet
    {
        public ResultSet()
        {
            Rows = new List<Row>();
        }

        public IList<Row> Rows { get; set; }

        public void AddRow(params ColumnValue[] columnValues)
        {
            var row = new Row(columnValues);
            Rows.Add(row);
        }
    }
}
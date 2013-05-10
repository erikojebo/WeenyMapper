using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Extensions;

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

        public override string ToString()
        {
            var rowStrings = Rows.Select(x => x.ToString());
            return string.Join(", ", rowStrings);
        }

        public ResultSet Join(ResultSet table, string leftTableIdentifier, string leftColumnName, string rightTableIdentifier, string rightColumnName)
        {
            var joinedRows = new List<Row>();

            foreach (var row in Rows)
            {
                var leftValue = row.GetColumnValue(leftTableIdentifier, leftColumnName);
                var matchingRows = table.Rows.Where(x => Equals(x.GetColumnValue(rightTableIdentifier, rightColumnName).Value, leftValue.Value));

                if (matchingRows.IsEmpty())
                    joinedRows.Add(row);

                foreach (var matchingRow in matchingRows)
                {
                    var combinedColumnValues = row.ColumnValues.Concat(matchingRow.ColumnValues);
                    joinedRows.Add(new Row(combinedColumnValues));
                }
            }

            return new ResultSet(joinedRows);
        }
    }
}
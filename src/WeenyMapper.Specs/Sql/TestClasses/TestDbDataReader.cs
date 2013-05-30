using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using WeenyMapper.Mapping;
using System.Linq;

namespace WeenyMapper.Specs.Sql
{
    public class TestDbDataReader : DbDataReader
    {
        public ResultSet ResultSet { get; set; }
        public int CurrentRowIndex { get; set; }

        public TestDbDataReader()
        {
            CurrentRowIndex = -1;
        }

        public override void Close()
        {
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public override bool NextResult()
        {
            CurrentRowIndex += 1;

            return CurrentRowIndex < ResultSet.Rows.Count;
        }

        public override bool Read()
        {
            return NextResult();
        }

        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public override int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public override bool GetBoolean(int ordinal)
        {
            return (bool)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override byte GetByte(int ordinal)
        {
            return (byte)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return (long)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override char GetChar(int ordinal)
        {
            return (char)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return (long)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override Guid GetGuid(int ordinal)
        {
            return (Guid)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override short GetInt16(int ordinal)
        {
            return (short)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override int GetInt32(int ordinal)
        {
            return (int)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override long GetInt64(int ordinal)
        {
            return (long)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override string GetString(int ordinal)
        {
            return (string)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override object GetValue(int ordinal)
        {
            return ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int ordinal)
        {
            return Equals(ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value, null);
        }

        public override int FieldCount
        {
            get { return ResultSet.Rows[CurrentRowIndex].ColumnValues.Count; }
        }

        public override object this[int ordinal]
        {
            get { return ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value; }
        }

        public override object this[string name]
        {
            get { return CurrentRow.GetColumnValue(name).Value; }
        }

        private Row CurrentRow
        {
            get { return ResultSet.Rows[CurrentRowIndex]; }
        }

        public override bool HasRows
        {
            get { return ResultSet.Rows.Any(); }
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override double GetDouble(int ordinal)
        {
            return (double)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override float GetFloat(int ordinal)
        {
            return (float)ResultSet.Rows[CurrentRowIndex].ColumnValues[ordinal].Value;
        }

        public override string GetName(int ordinal)
        {
            return Cell(ordinal).ColumnName;
        }

        public override int GetOrdinal(string name)
        {
            var existing = CurrentRow.ColumnValues.FirstOrDefault(x => x.ColumnName == name);
            return CurrentRow.ColumnValues.IndexOf(existing);
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            return Cell(ordinal).Value.GetType();
        }

        private ColumnValue Cell(int ordinal)
        {
            return CurrentRow.ColumnValues[ordinal];
        }

        public override IEnumerator GetEnumerator()
        {
            return ResultSet.Rows.GetEnumerator();
        }
    }
}
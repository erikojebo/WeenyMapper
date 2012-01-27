using System;

namespace WeenyMapper.Mapping
{
    public class ColumnValue 
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public ColumnValue(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
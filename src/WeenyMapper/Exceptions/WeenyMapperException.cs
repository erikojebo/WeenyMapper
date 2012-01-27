using System;

namespace WeenyMapper.Exceptions
{
    public class WeenyMapperException : Exception
    {
        public WeenyMapperException(string message) : base(message) {}
        public WeenyMapperException(string format, params object[] values) : base(string.Format(format, values)) {}
    }
}
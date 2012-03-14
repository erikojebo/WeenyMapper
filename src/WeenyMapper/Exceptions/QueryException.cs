namespace WeenyMapper.Exceptions
{
    public class QueryException : WeenyMapperException
    {
        public QueryException(string message) : base(message) {}
        public QueryException(string format, params object[] values) : base(format, values) {}
    }
}
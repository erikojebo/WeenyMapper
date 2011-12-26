namespace WeenyMapper.QueryParsing
{
    public interface IQueryParser
    {
        SelectQuery ParseSelectQuery(string methodName);
    }
}
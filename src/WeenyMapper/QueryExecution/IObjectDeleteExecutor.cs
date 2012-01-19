namespace WeenyMapper.QueryExecution
{
    public interface IObjectDeleteExecutor
    {
        string ConnectionString { get; set; }
        int Delete<T>(T instance);
    }
}
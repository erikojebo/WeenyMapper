namespace WeenyMapper.QueryExecution
{
    public interface IObjectUpdateExecutor {
        string ConnectionString { get; set; }
        void Update<T>(T instance);
    }
}
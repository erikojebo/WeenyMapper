namespace WeenyMapper.Sql
{
    public interface ICommandParameterFactory {
        CommandParameter Create(string columnName, object value);
    }
}
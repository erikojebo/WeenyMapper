namespace WeenyMapper.Conventions
{
    public class DefaultConvention : IConvention
    {
        public string GetColumnName(string propertyName)
        {
            return propertyName;
        }

        public string GetTableName(string className)
        {
            return className;
        }
    }
}
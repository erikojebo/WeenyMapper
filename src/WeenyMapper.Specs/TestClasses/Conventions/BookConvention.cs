using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class BookConvention : IConvention
    {
        public string GetColumnName(string propertyName)
        {
            return "c_" + propertyName.ToUpper();
        }

        public string GetTableName(string className)
        {
            return "t_" + className + "s";
        }

        public bool IsIdProperty(string propertyName)
        {
            return propertyName == "Isbn";
        }
    }
}
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class UpperCaseConvention : DefaultConvention
    {
        public override string GetColumnName(string propertyName)
        {
            return propertyName.ToUpper();
        }

        public override string GetTableName(string className)
        {
            return className.ToUpper();
        }
    }
}
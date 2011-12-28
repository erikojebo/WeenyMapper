using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class UserConvention : DefaultConvention
    {
        public override string GetTableName(string className)
        {
            if (className == "PartialUser")
            {
                return base.GetTableName("User");
            }

            return base.GetTableName(className);
        }
    }
}
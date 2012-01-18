using System.Reflection;
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.TestClasses.Conventions
{
    public class UserWithExtraPropertiesConvention : DefaultConvention
    {
        public override string GetTableName(string className)
        {
            return "User";
        }

        public override bool ShouldMapProperty(PropertyInfo propertyInfo)
        {
            var getter = propertyInfo.GetGetMethod();

            return getter.IsPublic && !getter.IsStatic && propertyInfo.Name != "PublicExtraProperty";
        }
    }
}
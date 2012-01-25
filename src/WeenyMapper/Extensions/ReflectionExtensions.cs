using System;
using System.Linq;

namespace WeenyMapper.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool ImplementsGenericInterface(this Type type, Type genericInterfaceType)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterfaceType);
        }
    }
}
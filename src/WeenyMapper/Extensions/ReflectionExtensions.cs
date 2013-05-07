using System;
using System.Linq;
using System.Reflection;

namespace WeenyMapper.Extensions
{
    internal static class ReflectionExtensions
    {
        public static bool ImplementsGenericInterface(this Type type, Type genericInterfaceType)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterfaceType);
        }
        
        public static bool ImplementsInterface<T>(this Type type)
        {
            return type.GetInterfaces().Any(x => x == typeof(T));
        }
    }
}
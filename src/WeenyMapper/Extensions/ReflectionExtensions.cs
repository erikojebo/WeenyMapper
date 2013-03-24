using System;
using System.Linq;
using System.Reflection;

namespace WeenyMapper.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool ImplementsGenericInterface(this Type type, Type genericInterfaceType)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterfaceType);
        }

        public static bool HasProperty(this object o, string name)
        {
            return o.GetProperty(name) != null;
        }

        public static PropertyInfo GetProperty(this object o, string name)
        {
            return o.GetType().GetProperty(name);
        }
    }
}
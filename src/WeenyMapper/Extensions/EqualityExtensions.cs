using System;
using System.Linq.Expressions;
using WeenyMapper.Reflection;

namespace WeenyMapper.Extensions
{
    public static class EqualityExtensions
    {
        public static bool NullSafeIdEquals<T>(this T instance, T other, Expression<Func<T, object>> selector)
        {
            var property = Reflector<T>.GetProperty(selector);

            if (!Equals(instance, null) && !Equals(other, null))
            {
                return Equals(property.GetValue(instance, null), property.GetValue(other, null));
            }
            if (Equals(instance, null) && Equals(instance, null))
            {
                return true;
            }

            return false;
        }
    }
}
using System.Collections.Generic;

namespace WeenyMapper.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AsList<T>(this T instance)
        {
            return new[] { instance };
        }
    }
}
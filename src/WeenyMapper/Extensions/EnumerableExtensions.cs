using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AsList<T>(this T instance)
        {
            return new[] { instance };
        }

        public static bool ElementEquals<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            var leftCount = left.Count();
            var rightCount = right.Count();

            if (leftCount != rightCount)
            {
                return false;
            }

            return left.Zip(right, (x, y) => Equals(x, y)).All(x => x);
        }
    }
}
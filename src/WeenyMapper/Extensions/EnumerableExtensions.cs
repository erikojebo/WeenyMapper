using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.Extensions
{
    public static class EnumerableExtensions
    {
        public static IList<T> AsList<T>(this T instance)
        {
            return new List<T> { instance };
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

        public static void AddRange<T>(this ICollection<T> enumerable, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                enumerable.Add(item);
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }
    }
}
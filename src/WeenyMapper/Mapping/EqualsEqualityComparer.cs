using System.Collections.Generic;

namespace WeenyMapper.Mapping
{
    public class EqualsEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return object.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
using System;

namespace WeenyMapper.QueryParsing
{
    public abstract class EquatableQueryExpression<T> : QueryExpression, IEquatable<T> where T : class
    {
        public abstract override int GetHashCode();

        public override bool Equals(object obj)
        {
            return Equals(obj as T);
        }

        public bool Equals(T other)
        {
            if (Equals(other, null))
            {
                return false;
            }

            return NullSafeEquals(other);
        }

        protected abstract bool NullSafeEquals(T other);
    }
}
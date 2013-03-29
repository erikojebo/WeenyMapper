using System.Collections.Generic;
using WeenyMapper.Reflection;

namespace WeenyMapper.Mapping
{
    public class IdPropertyComparer<T> : IEqualityComparer<T> 
    {
        private readonly IConventionReader _conventionReader;

        public IdPropertyComparer(IConventionReader conventionReader)
        {
            _conventionReader = conventionReader;
        }

        public bool Equals(T x, T y)
        {
            var leftValue = _conventionReader.GetPrimaryKeyValue(x);
            var rightValue = _conventionReader.GetPrimaryKeyValue(y);

            // If the id of both entities is null we can't really say if they represent the same entity
            // or not, so treat them as different objects. Better to have duplicates than missing rows in
            // the result set, which would be the result of incorrectly assuming two rows of representing
            // the same entity.
            return Equals(leftValue, rightValue) && leftValue != null;
        }

        public int GetHashCode(T obj)
        {
            var primaryKeyValue = _conventionReader.GetPrimaryKeyValue(obj);

            if (primaryKeyValue == null)
                return 0;

            return primaryKeyValue.GetHashCode();
        }
    }
}
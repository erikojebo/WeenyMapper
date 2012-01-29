using System;
using System.Collections.Generic;
using WeenyMapper.Conventions;
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
            return Equals(_conventionReader.GetPrimaryKeyValue(x), _conventionReader.GetPrimaryKeyValue(y));
        }

        public int GetHashCode(T obj)
        {
            return _conventionReader.GetPrimaryKeyValue(obj).GetHashCode();
        }
    }
}
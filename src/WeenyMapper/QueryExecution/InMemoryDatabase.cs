using System;
using System.Collections.Generic;
using System.Linq;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryDatabase
    {
        private Dictionary<Type, List<object>> _entities = new Dictionary<Type, List<object>>();

        public List<T> Entities<T>()
        {
            return _entities[typeof(T)].OfType<T>().ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Mapping;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryDatabase
    {
        private readonly IConventionReader _conventionReader;
        private readonly IEntityMapper _entityMapper;
        private readonly Dictionary<Type, ResultSet> _tables = new Dictionary<Type, ResultSet>();

        public InMemoryDatabase(IConventionReader conventionReader, IEntityMapper entityMapper)
        {
            _conventionReader = conventionReader;
            _entityMapper = entityMapper;
        }

        public void Add<T>(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        private void Add<T>(T entity)
        {
            var columnValues = _conventionReader.GetColumnValues(entity);
            var row = new Row(columnValues);

            EnsureTable<T>();

            Table<T>().AddRow(row);
        }

        private void EnsureTable<T>()
        {
            if (!_tables.ContainsKey(typeof(T)))
                _tables[typeof(T)] = new ResultSet();
        }

        public IList<T> FindAll<T>()
        {
            EnsureTable<T>();

            return _entityMapper.CreateInstanceGraphs<T>(Table<T>());
        }

        private ResultSet Table<T>()
        {
            return _tables[typeof(T)];
        }
    }
}
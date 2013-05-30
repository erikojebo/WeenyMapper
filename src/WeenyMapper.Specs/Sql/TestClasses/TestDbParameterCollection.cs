using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Specs.Sql
{
    public class TestDbParameterCollection : DbParameterCollection
    {
        private readonly List<TestDbParameter> _parameters = new List<TestDbParameter>();

        private readonly object _syncroot = new object();

        public override int Add(object value)
        {
            var testDbParameter = Create(value);

            _parameters.Add(testDbParameter);

            return IndexOf(testDbParameter);
        }

        private static TestDbParameter Create(object value)
        {
            return new TestDbParameter() { Value = value };
        }

        public override bool Contains(object value)
        {
            return Find(value) != null;
        }

        public override void Clear()
        {
            _parameters.Clear();
        }

        public override int IndexOf(object value)
        {
            return _parameters.IndexOf(Find(value));
        }

        private TestDbParameter Find(object value)
        {
            return _parameters.FirstOrDefault(x => x.Matches(value));
        }

        public override void Insert(int index, object value)
        {
            _parameters.Insert(index, Create(value));
        }

        public override void Remove(object value)
        {
            _parameters.Remove(Find(value));
        }

        public override void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            _parameters.Remove(_parameters.FirstOrDefault(x => x.ParameterName == parameterName));
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameters[index] = value as TestDbParameter;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var existing = _parameters.FirstOrDefault(x => x.ParameterName == parameterName);

            if (existing != null)
                _parameters.Remove(existing);

            _parameters.Add(value as TestDbParameter);
        }

        public override int Count
        {
            get { return _parameters.Count; }
        }

        public override object SyncRoot
        {
            get { return _syncroot; }
        }

        public override bool IsFixedSize
        {
            get { return false; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsSynchronized
        {
            get { return false; }
        }

        public override int IndexOf(string parameterName)
        {
            var existing = FindForName(parameterName);
            return _parameters.IndexOf(existing);
        }

        private TestDbParameter FindForName(string parameterName)
        {
            return _parameters.FirstOrDefault(x => x.ParameterName == parameterName);
        }

        public override IEnumerator GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            return _parameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return FindForName(parameterName);
        }

        public override bool Contains(string value)
        {
            return Find(value) != null;
        }

        public override void CopyTo(Array array, int index)
        {
            _parameters.CopyTo(array as TestDbParameter[], index);
        }

        public override void AddRange(Array values)
        {
            foreach (TestDbParameter testDbParameter in values)
            {
                _parameters.Add(testDbParameter);
            }
        }
    }
}
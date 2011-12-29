using System;
using System.Collections.Generic;
using System.Dynamic;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicCountBuilder<T> : DynamicObject
    {
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private readonly IObjectCountExecutor _objectCountExecutor;

        public DynamicCountBuilder(IObjectCountExecutor objectCountExecutor)
        {
            _objectCountExecutor = objectCountExecutor;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var calledMethodName = binder.Name;

            if (calledMethodName.StartsWith("Where"))
            {
                var propertyName = calledMethodName.Substring("Where".Length);
                _constraints[propertyName] = args[0];
            }
            else
            {
                throw new InvalidOperationException("Unknown prefix");
            }

            result = this;
            return true;
        }

        public int Execute()
        {
            return _objectCountExecutor.Count<T>(_constraints);
        }
    }
}
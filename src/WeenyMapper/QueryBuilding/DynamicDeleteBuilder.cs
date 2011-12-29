using System;
using System.Collections.Generic;
using System.Dynamic;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public abstract class DynamicCommandBuilderBase : DynamicObject {}

    public class DynamicDeleteBuilder<T> : DynamicCommandBuilderBase
    {
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private readonly IObjectDeleteExecutor _objectDeleteExecutor;

        public DynamicDeleteBuilder(IObjectDeleteExecutor objectDeleteExecutor)
        {
            _objectDeleteExecutor = objectDeleteExecutor;
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

        public void Execute()
        {
            _objectDeleteExecutor.Delete<T>(_constraints);
        }
    }
}
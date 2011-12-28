using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicUpdateBuilder<T> : DynamicObject
    {
        private readonly IDictionary<string, object> _constraints = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _setters = new Dictionary<string, object>();
        private readonly IObjectUpdateExecutor _objectUpdateExecutor;

        public DynamicUpdateBuilder(IObjectUpdateExecutor objectUpdateExecutor)
        {
            _objectUpdateExecutor = objectUpdateExecutor;
        }

        public void Update(T instance)
        {
            _objectUpdateExecutor.Update(instance);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var calledMethodName = binder.Name;

            if (calledMethodName.StartsWith("Where"))
            {
                var propertyName = calledMethodName.Substring("Where".Length);
                _constraints[propertyName] = args[0];                
            }
            else if (calledMethodName.StartsWith("Set"))
            {
                var propertyName = calledMethodName.Substring("Set".Length);
                _setters[propertyName] = args[0];                
            }

            result = this;
            return true;
        }

        public void Execute()
        {
            _objectUpdateExecutor.Update<T>(_constraints, _setters);
        }
    }
}
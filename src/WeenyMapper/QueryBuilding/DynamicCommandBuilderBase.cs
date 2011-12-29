using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace WeenyMapper.QueryBuilding
{
    public abstract class DynamicCommandBuilderBase : DynamicObject
    {
        // Subject for refactoring... The dictionary from the abyss.
        // Stores (Property name, value) pairs for different method prefixes such as By, Where or Set.
        private readonly IDictionary<string, IDictionary<string, object>> _prefixToPropertyValuesMap =
            new Dictionary<string, IDictionary<string, object>>();

        protected abstract IEnumerable<string> ValidPrefixes { get; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            NormalizePrefixToPropertyValuesMap();

            var calledMethodName = binder.Name;

            var methodPrefix = ValidPrefixes.FirstOrDefault(calledMethodName.StartsWith);

            if (methodPrefix == null)
            {
                throw new InvalidOperationException("Failed to parse method name due to unknown method prefix: " + calledMethodName);
            }

            var propertyName = calledMethodName.Substring(methodPrefix.Length);

            _prefixToPropertyValuesMap[methodPrefix][propertyName] = args[0];

            result = this;
            return true;
        }

        private void NormalizePrefixToPropertyValuesMap()
        {
            foreach (var validPrefix in ValidPrefixes)
            {
                if (!_prefixToPropertyValuesMap.ContainsKey(validPrefix))
                {
                    _prefixToPropertyValuesMap[validPrefix] = new Dictionary<string, object>();
                }                
            }
        }

        protected IDictionary<string, object> GetPropertyValues(string prefix)
        {
            return _prefixToPropertyValuesMap[prefix];
        }
    }
}
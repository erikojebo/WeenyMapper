using System;
using System.Collections.Generic;

namespace WeenyMapper.Extensions
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TTransformedKey, TValue> TransformKeys<TKey, TValue, TTransformedKey>(
            this IDictionary<TKey, TValue> original, 
            Func<TKey, TTransformedKey> transformation)
        {
            var transformed = new Dictionary<TTransformedKey, TValue>();
            
            foreach (var pair in original)
            {
                var transformedKey = transformation(pair.Key);
                transformed[transformedKey] = pair.Value;
            }

            return transformed;
        }
    }
}
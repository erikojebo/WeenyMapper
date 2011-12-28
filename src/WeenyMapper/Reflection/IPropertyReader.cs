using System.Collections.Generic;

namespace WeenyMapper.Reflection
{
    public interface IPropertyReader
    {
        IDictionary<string, object> GetColumnValues(object instance);
        IDictionary<string, object> GetPropertyValues(object instance);
    }
}
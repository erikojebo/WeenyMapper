using System.Reflection;

namespace WeenyMapper.Sql
{
    public class ObjectQueryJoinSpecification
    {
        public ObjectQuerySpecification ObjectQuerySpecification { get; set; }
        public PropertyInfo ParentProperty { get; set; }
        public PropertyInfo ChildProperty { get; set; }
    }
}
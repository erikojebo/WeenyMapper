using System;

namespace WeenyMapper.Sql
{
    public class AliasedObjectSubQuery
    {
        public AliasedObjectSubQuery(Type resultType)
        {
            ResultType = resultType;
        }

        public Type ResultType { get; set; }
        public string Alias { get; set; }
    }
}
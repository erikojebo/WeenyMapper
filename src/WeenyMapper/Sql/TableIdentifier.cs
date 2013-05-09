using System;
using WeenyMapper.Conventions;
using WeenyMapper.Reflection;

namespace WeenyMapper.Sql
{
    public class TableIdentifier
    {
        private readonly Type _type;
        private readonly string _alias;

        public TableIdentifier(Type type, string alias)
        {
            _type = type;
            _alias = alias;
        }

        public string GetTableIdentifier(IConventionReader conventionReader)
        {
            return _alias ?? conventionReader.GetTableName(_type);
        }
    }
}
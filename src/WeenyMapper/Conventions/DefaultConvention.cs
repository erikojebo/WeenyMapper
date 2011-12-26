using System;

namespace WeenyMapper.Conventions
{
    public class DefaultConvention : IConvention 
    {
        public string GetColumnName(string propertyName)
        {
            return Escape(propertyName);
        }

        public string GetTableName(string className)
        {
            return Escape(className);
        }

        private string Escape(string propertyName)
        {
            return string.Format("[{0}]", propertyName);
        }
    }
}
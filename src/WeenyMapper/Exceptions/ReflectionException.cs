using System;

namespace WeenyMapper.Exceptions
{
    public class ReflectionException : WeenyMapperException
    {
        public ReflectionException(string message) : base(message) {}
        public ReflectionException(string format, params object[] values) : base(format, values) {}
    }

    public class MissingDefaultConstructorException : ReflectionException
    {
        public MissingDefaultConstructorException(Type type)
            : base("Could not create an instance of the type '{0}' since it does not have a public default constructor", type.FullName) {}
    }

    public class MissingPropertyException : ReflectionException
    {
        private MissingPropertyException(string format, params object[] values) : base(format, values) {}

        public static MissingPropertyException CreateFromPropertyName(Type type, string propertyName)
        {
            return new MissingPropertyException("Could not find the property '{0}' in the type '{1}'", propertyName, type.FullName);
        }

        public static MissingPropertyException CreateFromColumnName(Type type, string columnName)
        {
            return new MissingPropertyException("Could not find any property in the type '{0}' matching the column name or alias '{1}' ", type.FullName, columnName);
        }
    }
}
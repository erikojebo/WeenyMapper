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
        public MissingPropertyException(Type type, string propertyName)
            : base("Could not find the property '{0}' in the type '{1}'", type.FullName, propertyName) {}
    }
}
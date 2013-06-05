using System;

namespace WeenyMapper.Specs.Exceptions
{
    public class SpecException : Exception
    {
        public SpecException()
        {
            
        }

        public SpecException(string message) : base(message)
        {
            
        }
    }
}
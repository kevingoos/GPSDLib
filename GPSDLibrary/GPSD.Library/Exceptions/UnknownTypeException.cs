using System;

namespace GPSD.Library.Exceptions
{
    public class UnknownTypeException : Exception
    {
        public UnknownTypeException() : base("Unknown Class Type")
        {
            
        }
    }
}
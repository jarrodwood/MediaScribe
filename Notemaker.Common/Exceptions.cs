using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.MediaScribe
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message) { }
        public ApplicationException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class DataAccessException : ApplicationException
    {
        public DataAccessException(string message) : base(message) { }
        public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class ValidationException<T> : ApplicationException
    {
        public T ValidatedObject { get; set; }
        public ValidationException(string message, T validatedObject)
            : base(message)
        {
            ValidatedObject = validatedObject;
        }
    }
}

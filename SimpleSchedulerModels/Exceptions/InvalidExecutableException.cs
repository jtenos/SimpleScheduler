using System;

namespace SimpleSchedulerModels.Exceptions
{
    public class InvalidExecutableException : ApplicationException
    {
        public InvalidExecutableException(string message) : base(message) { }
    }
}
using System;

namespace SimpleSchedulerModels.Exceptions
{
    public class InvalidValidationKeyException
        : ApplicationException
    {
        public InvalidValidationKeyException() : base("Invalid validation key") { }
    }
}

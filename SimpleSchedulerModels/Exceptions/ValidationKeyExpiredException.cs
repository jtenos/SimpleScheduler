using System;

namespace SimpleSchedulerModels.Exceptions
{
    public class ValidationKeyExpiredException
        : ApplicationException
    {
        public ValidationKeyExpiredException() : base("Validation key expired, please try again") { }
    }
}

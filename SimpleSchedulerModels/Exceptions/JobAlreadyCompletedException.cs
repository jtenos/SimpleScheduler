using System;

namespace SimpleSchedulerModels.Exceptions
{
    public class JobAlreadyCompletedException
        : ApplicationException
    {
        public JobAlreadyCompletedException() : base("Job is already completed") { }
    }
}

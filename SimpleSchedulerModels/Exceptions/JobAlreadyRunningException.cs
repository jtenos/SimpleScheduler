using System;

namespace SimpleSchedulerModels.Exceptions
{
    public class JobAlreadyRunningException
        : ApplicationException
    {
        public JobAlreadyRunningException() : base("Job is already running") { }
    }
}

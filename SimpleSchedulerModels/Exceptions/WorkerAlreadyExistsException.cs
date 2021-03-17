using System;
namespace SimpleSchedulerModels.Exceptions
{
    public class WorkerAlreadyExistsException
        : ApplicationException
    {
        public WorkerAlreadyExistsException(string description)
            : base($"Worker with description [{description}] already exists.") { }
    }
}

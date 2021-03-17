using System;

namespace SimpleSchedulerModels.Exceptions
{
    public class CircularWorkerRelationshipException
        : ApplicationException
    {
        public CircularWorkerRelationshipException() : base("Circular worker relationship") { }
    }
}

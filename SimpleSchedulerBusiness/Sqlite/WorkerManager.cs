using System;
using SimpleSchedulerData;

namespace SimpleSchedulerBusiness.Sqlite
{
    // No special handling for Sqlite
    public class WorkerManager
        : WorkerManagerBase
    {
        public WorkerManager(DatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }
    }
}

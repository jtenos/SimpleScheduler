using System;
using SimpleSchedulerData;

namespace SimpleSchedulerBusiness.SqlServer
{
    // No special handling for SQL Server
    public class WorkerManager
        : WorkerManagerBase
    {
        public WorkerManager(DatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }
    }
}

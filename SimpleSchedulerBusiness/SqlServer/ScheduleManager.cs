using System;
using SimpleSchedulerData;

namespace SimpleSchedulerBusiness.SqlServer
{
    public class ScheduleManager
        : ScheduleManagerBase
    {
        public ScheduleManager(DatabaseFactory databaseFactory, IServiceProvider serviceProvider) 
            : base(databaseFactory, serviceProvider) { }
    }
}

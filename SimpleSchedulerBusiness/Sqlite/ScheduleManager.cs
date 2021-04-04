using System;
using SimpleSchedulerData;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class ScheduleManager
        : ScheduleManagerBase
    {
        public ScheduleManager(DatabaseFactory databaseFactory, IServiceProvider serviceProvider) 
            : base(databaseFactory, serviceProvider) { }
    }
}

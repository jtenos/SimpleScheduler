using System;
using Microsoft.Extensions.Configuration;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerBusiness.SqlServer
{
    // No special SQL Server stuff
    public class UserManager
        : UserManagerBase
    {
        public UserManager(DatabaseFactory databaseFactory, 
            IServiceProvider serviceProvider, IEmailer emailer, IConfiguration config) 
            : base(databaseFactory, serviceProvider, emailer, config) { }
    }
}

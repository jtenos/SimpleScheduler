using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerService
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            Trace.Listeners.Add(new CustomTraceListener(Path.Combine(logDir, "log.txt")));
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("secrets.json", optional: true)
                .Build();
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IConfiguration>(config);
                    services.AddTransient<IEmailer, Emailer>();
                    services.AddHostedService<Worker>();
                    services.AddScoped<JobScheduler>();
                    services.AddScoped<JobExecutor>();
                    services.AddScoped<DatabaseFactory>();
                    switch (config["DatabaseType"])
                    {
                        case "SqlServer":
                            services.AddScoped<BaseDatabase, SqlDatabase>();
                            services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.SqlServer.WorkerManager>();
                            services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.SqlServer.ScheduleManager>();
                            services.AddScoped<IJobManager, SimpleSchedulerBusiness.SqlServer.JobManager>();
                            services.AddScoped<IUserManager, SimpleSchedulerBusiness.SqlServer.UserManager>();
                            break;
                        case "Sqlite":
                            services.AddScoped<BaseDatabase, SqliteDatabase>();
                            services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.Sqlite.WorkerManager>();
                            services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.Sqlite.ScheduleManager>();
                            services.AddScoped<IJobManager, SimpleSchedulerBusiness.Sqlite.JobManager>();
                            services.AddScoped<IUserManager, SimpleSchedulerBusiness.Sqlite.UserManager>();
                            break;
                    }
                }).UseWindowsService().Build().RunAsync().ConfigureAwait(false);
        }
    }
}
/* 
INSTALLATION:

POWERSHELL:

(all on one line):
New-Service -Name Scheduler -BinaryPathName
c:\services\SimpleScheduler\SimpleSchedulerService.exe -DisplayName "Scheduler"
-Description "Scheduler" -StartupType Automatic
-Credential MyDomain\MyUser

net start Scheduler
*/
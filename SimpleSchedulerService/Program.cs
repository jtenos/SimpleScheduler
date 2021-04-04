using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.Sqlite;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerService
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.Listeners.Add(new CustomTraceListener(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "log.txt")));
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
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
                    services.AddScoped<BaseDatabase, SqliteDatabase>();
                    services.AddScoped<IJobManager, JobManager>();
                    services.AddScoped<IScheduleManager, ScheduleManager>();
                    services.AddScoped<IWorkerManager, WorkerManager>();
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
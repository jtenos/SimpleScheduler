using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.Sqlite;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerServiceChecker
{
    public class Program
    {
        public static async Task Main()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            Trace.TraceInformation("Creating host");
            await Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IConfiguration>(config);
                    services.AddScoped<DatabaseFactory>();
                    switch (config["DatabaseType"])
                    {
                        case "SqlServer":
                            services.AddScoped<BaseDatabase, SqlDatabase>();
                            services.AddScoped<IJobManager, SimpleSchedulerBusiness.SqlServer.JobManager>();
                            break;
                        case "Sqlite":
                            services.AddScoped<BaseDatabase, SqliteDatabase>();
                            services.AddScoped<IJobManager, SimpleSchedulerBusiness.Sqlite.JobManager>();
                            break;
                    }
                    services.AddScoped<IEmailer, Emailer>();
                }).UseWindowsService().Build().RunAsync().ConfigureAwait(false);
        }
    }
}

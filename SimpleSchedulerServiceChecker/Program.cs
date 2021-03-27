using System;
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
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            await Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IConfiguration>(config);
                    services.AddScoped<IDatabaseFactory, DatabaseFactory>();
                    services.AddScoped<IDatabase, SqlDatabase>();
                    services.AddScoped<IJobManager, JobManager>();
                    services.AddScoped<IEmailer, Emailer>();
                }).UseWindowsService().Build().RunAsync().ConfigureAwait(false);
        }
    }
}

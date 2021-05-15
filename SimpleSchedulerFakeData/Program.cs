using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Faker;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using SimpleSchedulerFakeData;
using SimpleSchedulerEmail;

var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("secrets.json", optional: true)
    .Build();
var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .AddTransient<IEmailer, FakeEmailer>()
    .AddScoped<DatabaseFactory>()
    .AddSingleton<Program>();
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
var serviceCollection = services.BuildServiceProvider();

await serviceCollection.GetRequiredService<Program>().GoAsync();

class Program
{
    private readonly IWorkerManager _workerManager;
    private readonly IScheduleManager _scheduleManager;
    private readonly IJobManager _jobManager;
    private readonly DatabaseFactory _databaseFactory;
    public Program(DatabaseFactory databaseFactory, IWorkerManager workerManager, IScheduleManager scheduleManager, IJobManager jobManager)
        => (_databaseFactory, _workerManager, _scheduleManager, _jobManager) = (databaseFactory, workerManager, scheduleManager, jobManager);

    public async Task GoAsync()
    {
        var workerIDs = new List<long>();
        for (int i = 0; i < 100; ++i)
        {
            Console.WriteLine($"Worker {i}");
            long workerID = await _workerManager.AddWorkerAsync(isActive: true,
                workerName: string.Join(" ", Lorem.Words(3)),
                detailedDescription: "",
                emailOnSuccess: "",
                parentWorkerID: null,
                timeoutMinutes: 20,
                directoryName: "Hello",
                executable: "Hello",
                argumentValues: "",
                cancellationToken: default);
            workerIDs.Add(workerID);
        }

        var rand = new Random();
        var scheduleIDs = new List<long>();
        foreach (long workerID in workerIDs)
        {
            Console.WriteLine($"Schedules for worker {workerID}");
            int numSchedules = rand.Next(0, 4);
            for (int i = 0; i < numSchedules; ++i)
            {
                long scheduleID = await _scheduleManager.AddScheduleAsync(
                    workerID: workerID,
                    isActive: true,
                    sunday: rand.Next(0, 2) == 1,
                    monday: rand.Next(0, 2) == 1,
                    tuesday: rand.Next(0, 2) == 1,
                    wednesday: rand.Next(0, 2) == 1,
                    thursday: rand.Next(0, 2) == 1,
                    friday: rand.Next(0, 2) == 1,
                    saturday: rand.Next(0, 2) == 1,
                    timeOfDayUTC: null,
                    recurTime: TimeSpan.FromHours(2),
                    recurBetweenStartUTC: null,
                    recurBetweenEndUTC: null,
                    oneTime: false,
                    cancellationToken: default
                );
                scheduleIDs.Add(scheduleID);
            }
        }

        foreach (long scheduleID in scheduleIDs)
        {
            Console.WriteLine($"Jobs for schedule {scheduleID}");
            int numJobs = rand.Next(0, 40);
            for (int i = 0; i < numJobs; ++i)
            {
                await _jobManager.AddJobAsync(scheduleID, DateTime.Now.AddMinutes(rand.Next(10000)), cancellationToken: default);
            }
        }
        await (await _databaseFactory.GetDatabaseAsync(default)).CommitAsync(default);
    }
}

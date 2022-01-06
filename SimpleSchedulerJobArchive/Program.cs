using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using System.Collections.Immutable;

namespace SimpleSchedulerJobArchive;

/// <summary>
/// You can run this to move old jobs out of the main table into an archive table.
/// It will compress the detailed message, which may result in significant savings if you're storing
/// a lot of text in the message.
/// 
/// Usage:
/// SimpleSchedulerJobArchive.exe 90
/// where 90 is the number of days you want to go back to archive.
/// You can run this manually or you can schedule it like a regular job with the scheduler.
/// 
/// Or you can run:
/// SimpleSchedulerJobArchive.exe --show-archive-message 1234
/// And you'll get the decompressed message for JobArchiveID=1234
/// </summary>
internal class Program
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Program(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(x => x.AddJsonFile("secrets.json", optional: true))
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<Program>();
                services.AddScoped<DatabaseFactory>();
                switch (context.Configuration["DatabaseType"])
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
            })
            .Build();

        await host.Services.GetRequiredService<Program>().GoAsync(args);
    }

    public async Task GoAsync(string[] args)
    {
        if (args is { Length: 2 } && args[0] == "--show-archive-message")
        {
            long jobArchiveID = long.Parse(args[1]);
            await ShowJobArchiveMessage(jobArchiveID);
            return;
        }

        if (args is not { Length: > 0 } || !int.TryParse(args[0], out int numDays) || numDays < 0)
        {
            numDays = 90;
        }

        ImmutableArray<long> jobIDs;
        IServiceScope scope = _scopeFactory.CreateScope();
        try
        {
            IJobManager jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
            jobIDs = await jobManager.GetOldJobIDsAsync(numDays, cancellationToken: default);
        }
        finally
        {
            await ((IAsyncDisposable)scope).DisposeAsync();
        }

        foreach (long jobID in jobIDs)
        {
            scope = _scopeFactory.CreateScope();
            try
            {
                IJobManager jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();

                Console.WriteLine($"Archiving Job {jobID}");
                await jobManager.ArchiveJobAsync(jobID, cancellationToken: default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        Console.WriteLine("Done");
    }

    private async Task ShowJobArchiveMessage(long jobArchiveID)
    {
        IServiceScope scope = _scopeFactory.CreateScope();
        try
        {
            IJobManager jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();

            string detailedMessage = await jobManager.GetArchivedDetailedMessageAsync(jobArchiveID, cancellationToken: default);
            if (string.IsNullOrWhiteSpace(detailedMessage))
            {
                Console.WriteLine("No message found");
                return;
            }
            Console.WriteLine(detailedMessage);
        }
        finally
        {
            await ((IAsyncDisposable)scope).DisposeAsync();
        }
    }
}

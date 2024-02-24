using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Data;

namespace SimpleSchedulerFakeData;

internal class Worker
    : BackgroundService
{
    private readonly IConfiguration _config;

    public Worker(IConfiguration config)
    {
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using SqlConnection conn = new(_config.GetConnectionString("SimpleScheduler"));
        await conn.OpenAsync(cancellationToken);

        Console.WriteLine("Clearing database");
        using (SqlCommand comm = conn.CreateCommand())
        {
            comm.CommandText = @"
                DELETE FROM [app].[Jobs];
                DELETE FROM [app].[Schedules];
                DELETE FROM [app].[Workers];
                DELETE FROM [app].[LoginAttempts];
                DELETE FROM [app].[Users];
            ";
            await comm.ExecuteNonQueryAsync(cancellationToken);
        }

        Console.WriteLine("Inserting user");
        using (SqlCommand comm = conn.CreateCommand())
        {
            comm.CommandText = @"
                INSERT INTO [app].[Users] ([EmailAddress])
                SELECT 'test@example.com'
                WHERE NOT EXISTS (SELECT TOP 1 1 FROM [app].[Users] WHERE [EmailAddress] = 'test@example.com');
            ";
            await comm.ExecuteNonQueryAsync(cancellationToken);
        }

        List<long> workerIDs = new();
        for (int i = 0; i < 100; ++i)
        {
            Console.WriteLine($"Inserting worker {i}");
            using SqlCommand comm = conn.CreateCommand();
            comm.CommandText = $@"
                INSERT INTO [app].[Workers] (
                    [WorkerName]
                    ,[DetailedDescription]
                    ,[EmailOnSuccess]
                    ,[TimeoutMinutes]
                    ,[DirectoryName]
                    ,[Executable]
                    ,[ArgumentValues]
                ) VALUES (
                    '{new Faker().Company.CompanyName().Replace("'", "")}'
                    ,''
                    ,''
                    ,20
                    ,'Hello'
                    ,'Hello'
                    ,''
                );
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT) [WorkerID]
            ";
            long workerID = (long)(await comm.ExecuteScalarAsync(cancellationToken))!;
            workerIDs.Add(workerID);
        }

        Random rand = new();
        List<long> scheduleIDs = new();
        foreach (long workerID in workerIDs)
        {
            int numSchedules = rand.Next(0, 4);
            for (int i = 0; i < numSchedules; ++i)
            {
                Console.WriteLine($"Inserting schedule {i} for worker {workerID}");
                using SqlCommand comm = conn.CreateCommand();
                comm.CommandText = $@"
                    INSERT INTO [app].[Schedules] (
                        [WorkerID]
                        ,[Sunday]
                        ,[Monday]
                        ,[Tuesday]
                        ,[Wednesday]
                        ,[Thursday]
                        ,[Friday]
                        ,[Saturday]
                        ,[RecurTime]
                    ) VALUES (
                        {workerID}
                        ,{rand.Next(0, 2)}
                        ,{rand.Next(0, 2)}
                        ,{rand.Next(0, 2)}
                        ,{rand.Next(0, 2)}
                        ,{rand.Next(0, 2)}
                        ,{rand.Next(0, 2)}
                        ,{rand.Next(0, 2)}
                        ,'02:00:00'
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) [ScheduleID]
                ";
                long scheduleID = (long)(await comm.ExecuteScalarAsync(cancellationToken))!;
                scheduleIDs.Add(scheduleID);
            }
        }

        foreach (long scheduleID in scheduleIDs)
        {
            int numJobs = rand.Next(0, 40);
            for (int i = 0; i < numJobs; ++i)
            {
                Console.WriteLine($"Inserting job {i} for schedule {scheduleID}");
                using SqlCommand comm = conn.CreateCommand();
                comm.CommandText = $@"
                    INSERT INTO [app].[Jobs] (
                        [ScheduleID]
                        ,[QueueDateUTC]
                    ) VALUES (
                        {scheduleID}
                        ,@QueueDateUTC
                    );
                ";
                comm.Parameters.Add(new SqlParameter("@QueueDateUTC", SqlDbType.DateTime2) { Value = DateTime.UtcNow.AddMinutes(rand.Next(10000)) });
                await comm.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        Console.WriteLine("DONE");
        Environment.Exit(0);
    }
}

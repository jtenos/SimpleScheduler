using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SimpleSchedulerSqliteDB;
using System.Data.Common;

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
        bool sqlite = string.Equals(_config["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase);

        using DbConnection conn = sqlite
            ? new SqliteConnection(BuildSqliteConnectionString())
            : new SqlConnection(_config.GetConnectionString("SimpleScheduler"));
        await conn.OpenAsync(cancellationToken);

        if (sqlite)
        {
            await SqliteSchemaInitializer.EnsureSchemaAsync(conn);
        }

        // SQLite has no schemas, so table names are unqualified; otherwise use the [app] schema.
        string jobs = sqlite ? "Jobs" : "[app].[Jobs]";
        string schedules = sqlite ? "Schedules" : "[app].[Schedules]";
        string workers = sqlite ? "Workers" : "[app].[Workers]";
        string loginAttempts = sqlite ? "LoginAttempts" : "[app].[LoginAttempts]";
        string users = sqlite ? "Users" : "[app].[Users]";
        string lastIdentity = sqlite ? "last_insert_rowid()" : "CAST(SCOPE_IDENTITY() AS BIGINT)";

        Console.WriteLine("Clearing database");
        await ExecuteAsync(conn,
            $"DELETE FROM {jobs}; DELETE FROM {schedules}; DELETE FROM {workers}; DELETE FROM {loginAttempts}; DELETE FROM {users};",
            cancellationToken);

        Console.WriteLine("Inserting user");
        string userExists = sqlite
            ? $"SELECT 1 FROM {users} WHERE EmailAddress = 'test@example.com'"
            : $"SELECT TOP 1 1 FROM {users} WHERE [EmailAddress] = 'test@example.com'";
        await ExecuteAsync(conn,
            $"INSERT INTO {users} (EmailAddress) SELECT 'test@example.com' WHERE NOT EXISTS ({userExists});",
            cancellationToken);

        List<long> workerIDs = new();
        for (int i = 0; i < 100; ++i)
        {
            Console.WriteLine($"Inserting worker {i}");
            string companyName = new Faker().Company.CompanyName().Replace("'", "");
            long workerID = await ExecuteScalarLongAsync(conn, $@"
                INSERT INTO {workers} (
                    WorkerName, DetailedDescription, EmailOnSuccess, TimeoutMinutes, DirectoryName, Executable, ArgumentValues
                ) VALUES (
                    '{companyName}', '', '', 20, 'Hello', 'Hello', ''
                );
                SELECT {lastIdentity};",
                cancellationToken);
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
                long scheduleID = await ExecuteScalarLongAsync(conn, $@"
                    INSERT INTO {schedules} (
                        WorkerID, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, RecurTime
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
                    SELECT {lastIdentity};",
                    cancellationToken);
                scheduleIDs.Add(scheduleID);
            }
        }

        foreach (long scheduleID in scheduleIDs)
        {
            int numJobs = rand.Next(0, 40);
            for (int i = 0; i < numJobs; ++i)
            {
                Console.WriteLine($"Inserting job {i} for schedule {scheduleID}");
                DateTime queueDateUTC = DateTime.UtcNow.AddMinutes(rand.Next(10000));

                using DbCommand comm = conn.CreateCommand();
                comm.CommandText = $"INSERT INTO {jobs} (ScheduleID, QueueDateUTC) VALUES ({scheduleID}, @QueueDateUTC);";
                DbParameter param = comm.CreateParameter();
                param.ParameterName = "@QueueDateUTC";
                // SQLite stores date/times as ISO-8601 text; SQL Server takes the DateTime directly.
                param.Value = sqlite
                    ? queueDateUTC.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
                    : queueDateUTC;
                comm.Parameters.Add(param);
                await comm.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        Console.WriteLine("DONE");
        Environment.Exit(0);
    }

    private string BuildSqliteConnectionString()
    {
        string path = _config["Database:SqlitePath"] is { Length: > 0 } configured
            ? configured
            : Path.Combine(AppContext.BaseDirectory, "SimpleScheduler.sqlite");
        return new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    private static async Task ExecuteAsync(DbConnection conn, string sql, CancellationToken cancellationToken)
    {
        using DbCommand comm = conn.CreateCommand();
        comm.CommandText = sql;
        await comm.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<long> ExecuteScalarLongAsync(DbConnection conn, string sql, CancellationToken cancellationToken)
    {
        using DbCommand comm = conn.CreateCommand();
        comm.CommandText = sql;
        object result = (await comm.ExecuteScalarAsync(cancellationToken))!;
        return Convert.ToInt64(result);
    }
}

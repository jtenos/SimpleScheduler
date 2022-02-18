using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.SqlServer;
using SimpleSchedulerData;
using System.Data;

namespace SimpleSchedulerTests;

class SqlServerTestSetup
{
    internal static async Task SetUpAsync(AppSettings appSettings)
    {
        using SqlConnection conn = GetConnection(appSettings.Database.ConnectionString);
        await conn.OpenAsync();
        using SqlCommand comm = conn.CreateCommand();
        comm.CommandText = @"
            DELETE Jobs;
            DBCC CHECKIDENT('dbo.Jobs', RESEED, 0);
            DELETE Schedules;
            DBCC CHECKIDENT('dbo.Schedules', RESEED, 0);
            DELETE Workers;
            DBCC CHECKIDENT('dbo.Workers', RESEED, 0);
            DELETE LoginAttempts;
            DBCC CHECKIDENT('dbo.LoginAttempts', RESEED, 0);
            DELETE Users;
        ";
        await comm.ExecuteNonQueryAsync();
    }

    internal static void AddDatabaseSpecificConfig(IConfigurationBuilder configBuilder)
        => configBuilder.AddJsonFile("secrets.sqlserver.json", optional: true);

    internal static void AddDatabaseSpecificServices(IServiceCollection sc)
        => sc.AddScoped<IWorkerManager, WorkerManager>()
            .AddScoped<IScheduleManager, ScheduleManager>()
            .AddScoped<IJobManager, JobManager>()
            .AddScoped<IUserManager, UserManager>()
            .AddScoped<BaseDatabase, SqlDatabase>();

    internal static SqlConnection GetConnection(string connectionString)
        => new(connectionString);

    internal static SqlParameter Int64Parameter(string name, long? value)
        => new(name, SqlDbType.BigInt) { Value = value };

    internal static SqlParameter StringParameter(string name, string? value, int? size)
        => new(name, SqlDbType.NVarChar) { Value = value };
}

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.Sqlite;
using SimpleSchedulerData;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace SimpleSchedulerTests
{
    class SqliteTestSetup
    {
        internal static async Task SetUpAsync(IConfiguration config)
        {
            SqliteConnectionStringBuilder builder = new(config.GetConnectionString("SimpleScheduler"));
            string databaseFileName = builder.DataSource;
            if (File.Exists(databaseFileName))
            {
                File.Delete(databaseFileName);
                await Task.Delay(50);
            }
            await SqliteDatabase.CreateDatabaseAsync(databaseFileName);
        }

        internal static void AddDatabaseSpecificConfig(IConfigurationBuilder configBuilder)
            => configBuilder.AddJsonFile("secrets.sqlite.json", optional: true);

        internal static void AddDatabaseSpecificServices(IServiceCollection sc)
            => sc.AddScoped<IWorkerManager, WorkerManager>()
                .AddScoped<IScheduleManager, ScheduleManager>()
                .AddScoped<IJobManager, JobManager>()
                .AddScoped<IUserManager, UserManager>()
                .AddScoped<BaseDatabase, SqliteDatabase>();

        internal static DbConnection GetConnection(string connectionString)
            => new SqliteConnection(connectionString);

        internal static DbParameter Int64Parameter(string name, long? value)
            => new SqliteParameter(name, SqliteType.Integer) { Value = value };

        internal static DbParameter StringParameter(string name, string? value, int? size)
            => new SqliteParameter(name, SqliteType.Text) { Value = value };
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.Sqlite;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerEntities;
using SimpleSchedulerModels.Exceptions;

// TODO: Many more test cases covering different scenarios
// TODO: Build better support for the yyyyMMddHHmmssfff and hhmmssfff constants so they can be converted back and forth more easily
namespace SimpleSchedulerTests.Sqlite
{
    [TestClass]
    public class JobManagerTests
    {
        private readonly IConfiguration _config;
        private readonly string _databaseFileName;

        public JobManagerTests()
        {
            _config = GetServiceProvider().GetRequiredService<IConfiguration>();
            string connectionString = _config.GetConnectionString("SimpleScheduler");
            SqliteConnectionStringBuilder builder = new(connectionString);
            _databaseFileName = builder.DataSource;
        }

        private static IServiceProvider GetServiceProvider()
            => new ServiceCollection()
                .AddSingleton<IConfiguration>(
                    new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build()
                )
                .AddScoped<IWorkerManager, WorkerManager>()
                .AddScoped<IScheduleManager, ScheduleManager>()
                .AddScoped<IJobManager, JobManager>()
                .AddScoped<BaseDatabase, SqliteDatabase>()
                .AddScoped<DatabaseFactory>()
                .AddScoped<IEmailer, FakeEmailer>()
                .BuildServiceProvider();

        [TestInitialize]
        public async Task SetUpAsync()
        {
            if (File.Exists(_databaseFileName))
            {
                File.Delete(_databaseFileName);
                Thread.Sleep(50);
            }
            await SqliteDatabase.CreateDatabaseAsync(_databaseFileName);
        }

        [TestMethod]
        public async Task RestartStuckJobs()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "RUN");
            await AssertJobStatusAsync(1, "RUN");

            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.RestartStuckJobsAsync(default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            await AssertJobStatusAsync(1, "NEW");
        }

        [TestMethod]
        public async Task AcknowledgeError()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "ERR");
            await AssertJobStatusAsync(1, "ERR");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.AcknowledgeErrorAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            await AssertJobStatusAsync(1, "ACK");
        }

        [TestMethod]
        public async Task AddJob()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.AddJobAsync(1, DateTime.UtcNow, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var job = await GetJobAsync(1);
            Assert.AreEqual(1, job.JobID);
            Assert.AreEqual(1, job.ScheduleID);
            var queueDate = DateTime.ParseExact(job.QueueDateUTC.ToString(), "yyyyMMddHHmmssfff", null);
            var insertDate = DateTime.ParseExact(job.InsertDateUTC.ToString(), "yyyyMMddHHmmssfff", null);
            Assert.IsTrue(DateTime.UtcNow.Subtract(queueDate).TotalSeconds < 10);
            Assert.IsTrue(DateTime.UtcNow.Subtract(insertDate).TotalSeconds < 10);
            Assert.AreEqual("NEW", job.StatusCode);
        }

        [TestMethod]
        public async Task GetJob()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                var job = await jobManager.GetJobAsync(1, default);
                Assert.AreEqual(1, job.JobID);
                Assert.AreEqual(1, job.ScheduleID);
                var queueDate = job.QueueDateUTC;
                var insertDate = job.InsertDateUTC;
                Assert.IsTrue(DateTime.UtcNow.Subtract(queueDate).TotalSeconds < 10);
                Assert.IsTrue(DateTime.UtcNow.Subtract(insertDate).TotalSeconds < 10);
                Assert.AreEqual("NEW", job.StatusCode);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task CancelJob()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "NEW");
            await AssertJobStatusAsync(1, "NEW");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.CancelJobAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            await AssertJobStatusAsync(1, "CAN");
        }

        [TestMethod]
        [ExpectedException(typeof(JobAlreadyRunningException))]
        public async Task CancelJobRunning()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "RUN");
            await AssertJobStatusAsync(1, "RUN");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.CancelJobAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(JobAlreadyCompletedException))]
        public async Task CancelJobSuccess()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "SUC");
            await AssertJobStatusAsync(1, "SUC");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.CancelJobAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }


        [TestMethod]
        [ExpectedException(typeof(JobAlreadyCompletedException))]
        public async Task CancelJobError()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "ERR");
            await AssertJobStatusAsync(1, "ERR");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.CancelJobAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(JobAlreadyCompletedException))]
        public async Task CancelJobAcknowledged()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "ACK");
            await AssertJobStatusAsync(1, "ACK");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.CancelJobAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task CompleteJobSuccess()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "RUN");
            await AssertJobStatusAsync(1, "RUN");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.CompleteJobAsync(1, success: true, "abc", default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var job = await GetJobAsync(1);
            Assert.AreEqual("SUC", job.StatusCode);
            Assert.AreEqual("abc", job.DetailedMessage);
        }

        [TestMethod]
        public async Task CompleteJobFail()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "RUN");
            await AssertJobStatusAsync(1, "RUN");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                await jobManager.CompleteJobAsync(1, success: false, "abc", default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var job = await GetJobAsync(1);
            Assert.AreEqual("ERR", job.StatusCode);
            Assert.AreEqual("abc", job.DetailedMessage);
        }

        [TestMethod]
        public async Task GetLatestJobs()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                var jobs = await jobManager.GetLatestJobsAsync(1, 10, null, null, false, default);
                Assert.AreEqual(1, jobs.Length);
                Assert.AreEqual(1, jobs[0].Job.JobID);
                Assert.AreEqual(1, jobs[0].Schedule.ScheduleID);
                Assert.AreEqual(1, jobs[0].Worker.WorkerID);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetOverdueJobs()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetJobStatusAsync(1, "RUN");
            await CreateJobAsync(1);
            await SetQueueDateAsync(2, DateTime.UtcNow.AddMinutes(-30));
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                var jobs = await jobManager.GetOverdueJobsAsync(default);
                Assert.AreEqual(1, jobs.Length);
                Assert.AreEqual(2, jobs[0].Job.JobID);
                Assert.AreEqual(1, jobs[0].Schedule.ScheduleID);
                Assert.AreEqual(1, jobs[0].Worker.WorkerID);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetLastQueuedJob()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await Task.Delay(10);
            await CreateJobAsync(1);
            await Task.Delay(10);
            await CreateJobAsync(1);
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                var lastQueued = await jobManager.GetLastQueuedJobAsync(1, default);
                Assert.AreEqual(3, lastQueued?.JobID);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetJobDetailedMessage()
        {
            await CreateWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateJobAsync(1);
            await SetDetailedMessageAsync(1, "asdf");
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                string? message = await jobManager.GetJobDetailedMessageAsync(1, default);
                Assert.AreEqual("asdf", message);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task DequeueScheduledJobs()
        {
            for (int i = 0; i < 4; ++i)
            {
                await CreateWorkerAsync();
                await CreateScheduleAsync(i + 1);
                await CreateJobAsync(i + 1);
                await Task.Delay(10);
            }
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                var dequeuedJobs = await jobManager.DequeueScheduledJobsAsync(default);
                Assert.AreEqual(3, dequeuedJobs.Length);
                Assert.IsTrue(new long[] { 1, 2, 3 }.SequenceEqual(dequeuedJobs.Select(x => x.Job.JobID)));
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        private async Task CreateJobAsync(int scheduleID)
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName}");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = $@"
                INSERT INTO Jobs (
                    ScheduleID 
                    , InsertDateUTC
                    , QueueDateUTC 
                    , AcknowledgementID 
                ) VALUES (
                    {scheduleID}
                    ,{DateTime.UtcNow:yyyyMMddHHmmssfff}
                    ,{DateTime.UtcNow:yyyyMMddHHmmssfff}
                    ,'{Guid.NewGuid():N}' 
                );
            ";
            await comm.ExecuteNonQueryAsync();
        }

        private async Task CreateScheduleAsync(int workerID)
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName}");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = $@"
                INSERT INTO Schedules (
                    WorkerID
                    , Sunday
                    , Monday
                    , Tuesday
                    , Wednesday
                    , Thursday
                    , Friday
                    , Saturday
                    , RecurTime
                ) VALUES (
                    {workerID}
                    ,1, 1, 1, 1, 1, 1, 1
                    ,010000000
                );
            ";
            await comm.ExecuteNonQueryAsync();
        }

        private async Task CreateWorkerAsync()
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName}");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = $@"
                INSERT INTO Workers (
                    IsActive, WorkerName, DetailedDescription, EmailOnSuccess
                    , ParentWorkerID, TimeoutMinutes, DirectoryName
                    , Executable, ArgumentValues
                ) VALUES (
                    1, 'test worker {Guid.NewGuid():N}', '', ''
                    , null, 20, 'test'
                    ,'test', ''
                );
            ";
            await comm.ExecuteNonQueryAsync();
        }

        private async Task SetQueueDateAsync(long jobID, DateTime queueDateUTC)
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName};");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"
                UPDATE Jobs SET QueueDateUTC = @QueueDateUTC WHERE JobID = @JobID;
            ";
            comm.Parameters.AddRange(new[]
            {
                new SqliteParameter("@QueueDateUTC", SqliteType.Integer) { Value = long.Parse(queueDateUTC.ToString("yyyyMMddHHmmssfff")) },
                new SqliteParameter("@JobID", SqliteType.Integer) { Value = jobID }
            });
            await comm.ExecuteNonQueryAsync();
        }

        private async Task SetDetailedMessageAsync(long jobID, string message)
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName};");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"
                UPDATE Jobs SET DetailedMessage = @DetailedMessage WHERE JobID = @JobID;
            ";
            comm.Parameters.AddRange(new[]
            {
                new SqliteParameter("@DetailedMessage", SqliteType.Text) { Value = message },
                new SqliteParameter("@JobID", SqliteType.Integer) { Value = jobID }
            });
            await comm.ExecuteNonQueryAsync();
        }
        private async Task SetJobStatusAsync(long jobID, string status)
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName};");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"
                UPDATE Jobs SET StatusCode = @StatusCode WHERE JobID = @JobID;
            ";
            comm.Parameters.AddRange(new[]
            {
                new SqliteParameter("@StatusCode", SqliteType.Text) { Value = status },
                new SqliteParameter("@JobID", SqliteType.Integer) { Value = jobID }
            });
            await comm.ExecuteNonQueryAsync();
        }

        private async Task AssertJobStatusAsync(int jobID, string status)
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName};");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"
                SELECT StatusCode FROM Jobs WHERE JobID = @JobID;
            ";
            comm.Parameters.AddRange(new[]
            {
                new SqliteParameter("@JobID", SqliteType.Integer) { Value = jobID }
            });
            Assert.AreEqual(status, (string)(await comm.ExecuteScalarAsync())!);
        }

        private async Task<JobEntity> GetJobAsync(int jobID)
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName};");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"
                SELECT * FROM Jobs WHERE JobID = @JobID;
            ";
            comm.Parameters.AddRange(new[]
            {
                new SqliteParameter("@JobID", SqliteType.Integer) { Value = jobID }
            });
            using var rdr = await comm.ExecuteReaderAsync();
            await rdr.ReadAsync();
            return Mapper.MapJob(rdr);
        }
    }
}

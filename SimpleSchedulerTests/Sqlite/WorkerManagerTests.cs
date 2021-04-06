using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

namespace SimpleSchedulerTests
{
    [TestClass]
    public class WorkerManagerTests
    {
        private readonly IConfiguration _config;
        private readonly string _databaseFileName;

        private const string VALID_EMAIL_ADDRESS = "joe@example.com";

        public WorkerManagerTests()
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
        public async Task WorkerManagerType()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                Assert.IsInstanceOfType(workerManager, typeof(WorkerManager));
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task RunNow()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                await CreateWorkerAsync();
                await workerManager.RunNowAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var schedules = await GetAllSchedulesAsync();
            Assert.AreEqual(1, schedules.Length);
            Assert.AreEqual(0, schedules[0].IsActive);
            Assert.AreEqual(1, schedules[0].ScheduleID);
            Assert.AreEqual(1, schedules[0].OneTime);
            Assert.AreEqual(1, schedules[0].WorkerID);
            Assert.AreEqual(7, schedules[0].Sunday
                + schedules[0].Monday
                + schedules[0].Tuesday
                + schedules[0].Wednesday
                + schedules[0].Thursday
                + schedules[0].Friday
                + schedules[0].Saturday
                );

            var jobs = await GetAllJobsAsync();
            Assert.AreEqual(1, jobs.Length);
            Assert.AreEqual("NEW", jobs[0].StatusCode);
            Assert.AreEqual(1, jobs[0].ScheduleID);
            Assert.AreEqual(1, jobs[0].JobID);
            Assert.IsNull(jobs[0].CompleteDateUTC);
            Assert.IsTrue(DateTime.UtcNow.Subtract(DateTime.ParseExact(jobs[0].InsertDateUTC.ToString(), "yyyyMMddHHmmssfff", null)).TotalSeconds < 10);
            Assert.IsTrue(DateTime.UtcNow.Subtract(DateTime.ParseExact(jobs[0].QueueDateUTC.ToString(), "yyyyMMddHHmmssfff", null)).TotalSeconds < 10);
        }

        [TestMethod]
        public async Task GetChildWorkerIDsByJob()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                await CreateWorkerAsync();
                await CreateChildWorkerAsync(1);
                await CreateChildWorkerAsync(1);
                await CreateScheduleAsync(1);
                await CreateJobAsync(1);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                var childWorkers = await workerManager.GetChildWorkerIDsByJobAsync(1, default);
                Assert.AreEqual(2, childWorkers.Length);
                Assert.IsTrue(new long[] { 2, 3 }.SequenceEqual(childWorkers.OrderBy(x => x)));
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetAllWorkerDetails()
        {
            await CreateWorkerAsync();
            await CreateInactiveWorkerAsync();
            await CreateScheduleAsync(1);
            await CreateScheduleAsync(1);
            await CreateScheduleAsync(2);
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                var details = await workerManager.GetAllWorkerDetailsAsync(default, getActive: true, getInactive: false);
                Assert.AreEqual(1, details.Length);
                Assert.AreEqual(1, details[0].Worker.WorkerID);
                Assert.AreEqual(2, details[0].Schedules.Length);
                Assert.IsTrue(new long[] { 1, 2 }.SequenceEqual(details[0].Schedules.Select(x => x.ScheduleID).OrderBy(x => x)));

                details = await workerManager.GetAllWorkerDetailsAsync(default, getActive: false, getInactive: true);
                Assert.AreEqual(1, details.Length);
                Assert.AreEqual(2, details[0].Worker.WorkerID);
                Assert.AreEqual(1, details[0].Schedules.Length);
                Assert.AreEqual(3, details[0].Schedules[0].ScheduleID);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetAllWorkers()
        {
            await CreateWorkerAsync();
            await CreateInactiveWorkerAsync();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                var allWorkers = await workerManager.GetAllWorkersAsync(default, getActive: true, getInactive: false);
                Assert.AreEqual(1, allWorkers.Length);
                Assert.AreEqual(1, allWorkers[0].WorkerID);
                allWorkers = await workerManager.GetAllWorkersAsync(default, getActive: false, getInactive: true);
                Assert.AreEqual(1, allWorkers.Length);
                Assert.AreEqual(2, allWorkers[0].WorkerID);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetWorker()
        {
            await CreateWorkerAsync();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                var worker = await workerManager.GetWorkerAsync(1, default);
                Assert.AreEqual(1, worker.WorkerID);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task AddWorker()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                await workerManager.AddWorkerAsync(
                    isActive: true,
                    workerName: "Some Worker",
                    detailedDescription: "Hello!",
                    emailOnSuccess: "",
                    parentWorkerID: null,
                    timeoutMinutes: 20,
                    directoryName: "test",
                    executable: "hello there",
                    argumentValues: "args args",
                    default
                );
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var workers = await GetAllWorkersAsync();
            Assert.AreEqual(1, workers.Length);
            Assert.AreEqual(1, workers[0].WorkerID);
            Assert.AreEqual(1, workers[0].IsActive);
            Assert.AreEqual("Some Worker", workers[0].WorkerName);
            Assert.AreEqual("Hello!", workers[0].DetailedDescription);
            Assert.AreEqual("", workers[0].EmailOnSuccess);
            Assert.AreEqual(20, workers[0].TimeoutMinutes);
            Assert.IsNull(workers[0].ParentWorkerID);
            Assert.AreEqual("test", workers[0].DirectoryName);
            Assert.AreEqual("hello there", workers[0].Executable);
            Assert.AreEqual("args args", workers[0].ArgumentValues);
        }

        [TestMethod]
        public async Task UpdateWorker()
        {
            await CreateWorkerAsync();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                await workerManager.UpdateWorkerAsync(1, isActive: true,
                    workerName: "new name", detailedDescription: "",
                    emailOnSuccess: "", parentWorkerID: null, timeoutMinutes: 20,
                    directoryName: "some directory", executable: "abc",
                    argumentValues: "args args", default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var allWorkers = await GetAllWorkersAsync();
            Assert.AreEqual(1, allWorkers.Length);
            Assert.AreEqual("new name", allWorkers[0].WorkerName);
        }

        [TestMethod]
        public async Task DeactivateWorker()
        {
            await CreateWorkerAsync();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                await workerManager.DeactivateWorkerAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var allWorkers = await GetAllWorkersAsync();
            Assert.AreEqual(1, allWorkers.Length);
            Assert.AreEqual(0, allWorkers[0].IsActive);
            Assert.IsTrue(new Regex(@"^INACTIVE\: [0-9]{14} test worker [0-9a-f]{32}$").IsMatch(allWorkers[0].WorkerName));
        }

        [TestMethod]
        public async Task ReactivateWorker()
        {
            await CreateWorkerAsync();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                await workerManager.DeactivateWorkerAsync(1, default);
                await workerManager.ReactivateWorkerAsync(1, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
            var allWorkers = await GetAllWorkersAsync();
            Assert.AreEqual(1, allWorkers.Length);
            Assert.AreEqual(1, allWorkers[0].IsActive);
            Assert.IsTrue(new Regex(@"^test worker [0-9a-f]{32} \(react [0-9]{14}\)$").IsMatch(allWorkers[0].WorkerName));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularWorkerRelationshipException))]
        public async Task CircularWorkers()
        {
            await CreateWorkerAsync();
            await CreateWorkerAsync();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
                await workerManager.UpdateWorkerAsync(2, isActive: true,
                    workerName: "asdf", detailedDescription: "asdfads",
                    emailOnSuccess: "", parentWorkerID: 1, timeoutMinutes: 20,
                    directoryName: "asdf", executable: "asdf", argumentValues: "asdf",
                    default);
                await workerManager.UpdateWorkerAsync(1, isActive: true,
                    workerName: "noniun", detailedDescription: "asdfads",
                    emailOnSuccess: "", parentWorkerID: 2, timeoutMinutes: 20,
                    directoryName: "asdf", executable: "asdf", argumentValues: "asdf",
                    default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task TestRollback()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();

                // This one passes but the transaction doesn't get committed
                await workerManager.AddWorkerAsync(isActive: true, workerName: "abc",
                    detailedDescription: "", emailOnSuccess: "", parentWorkerID: null,
                    timeoutMinutes: 20, directoryName: "test", executable: "test",
                    argumentValues: "args", default);

                // This should fail and roll back the entire transaction
                await workerManager.AddWorkerAsync(isActive: true, workerName: "gjghjghj",
                    detailedDescription: "", emailOnSuccess: "", parentWorkerID: 34,
                    timeoutMinutes: 20, directoryName: "test", executable: "test",
                    argumentValues: "args", default);
            }
            catch
            {
                scope.ServiceProvider.GetRequiredService<DatabaseFactory>().MarkForRollback();
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }

            var workers = await GetAllWorkersAsync();
            Assert.AreEqual(0, workers.Length);
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

        private async Task CreateInactiveWorkerAsync()
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName}");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"
                INSERT INTO Workers (
                    IsActive, WorkerName, DetailedDescription, EmailOnSuccess
                    , ParentWorkerID, TimeoutMinutes, DirectoryName
                    , Executable, ArgumentValues
                ) VALUES (
                    0, 'test inactive worker', '', ''
                    , null, 20, 'test'
                    ,'test', ''
                );
            ";
            await comm.ExecuteNonQueryAsync();
        }

        private async Task CreateChildWorkerAsync(int parentWorkerID)
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
                    1, 'test worker {Guid.NewGuid()}', '', ''
                    , {parentWorkerID}, 20, 'test'
                    ,'test', ''
                );
            ";
            await comm.ExecuteNonQueryAsync();
        }

        private async Task<ImmutableArray<ScheduleEntity>> GetAllSchedulesAsync()
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName}");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"SELECT * FROM Schedules;";
            using var rdr = await comm.ExecuteReaderAsync();
            var result = new List<ScheduleEntity>();
            while (await rdr.ReadAsync())
            {
                result.Add(Mapper.MapSchedule(rdr));
            }
            return result.ToImmutableArray();
        }

        private async Task<ImmutableArray<JobEntity>> GetAllJobsAsync()
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName}");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"SELECT * FROM Jobs;";
            using var rdr = await comm.ExecuteReaderAsync();
            var result = new List<JobEntity>();
            while (await rdr.ReadAsync())
            {
                result.Add(Mapper.MapJob(rdr));
            }
            return result.ToImmutableArray();
        }

        private async Task<ImmutableArray<WorkerEntity>> GetAllWorkersAsync()
        {
            using var conn = new SqliteConnection($"Data Source={_databaseFileName}");
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = @"SELECT * FROM Workers;";
            using var rdr = await comm.ExecuteReaderAsync();
            var result = new List<WorkerEntity>();
            while (await rdr.ReadAsync())
            {
                result.Add(Mapper.MapWorker(rdr));
            }
            return result.ToImmutableArray();
        }
    }
}

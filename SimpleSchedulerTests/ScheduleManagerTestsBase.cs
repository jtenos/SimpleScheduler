//using System;
//using System.Data.Common;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SimpleSchedulerBusiness;
//using SimpleSchedulerData;
//using SimpleSchedulerEmail;
//using SimpleSchedulerEntities;

//namespace SimpleSchedulerTests
//{
//    public abstract class ScheduleManagerTestsBase
//    {
//        public ScheduleManagerTestsBase()
//        {
//            Config = GetServiceProvider().GetRequiredService<IConfiguration>();
//        }

//        protected IConfiguration Config { get; }

//        protected IServiceProvider GetServiceProvider()
//        {
//            var configBuilder = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json");

//            AddDatabaseSpecificConfig(configBuilder);

//            var config = configBuilder.Build();

//            var sc = new ServiceCollection()
//                .AddSingleton<IConfiguration>(config).AddScoped<DatabaseFactory>()
//                .AddScoped<IEmailer, FakeEmailer>();

//            AddDatabaseSpecificServices(sc);
//            return sc.BuildServiceProvider();
//        }

//        protected abstract void AddDatabaseSpecificServices(IServiceCollection sc);
//        protected abstract void AddDatabaseSpecificConfig(IConfigurationBuilder configBuilder);

//        [TestMethod]
//        public async Task DeactivateSchedule()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var scheduleManager = scope.ServiceProvider.GetRequiredService<IScheduleManager>();
//                await scheduleManager.DeactivateScheduleAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            var sched = await GetScheduleAsync(1);
//            Assert.AreEqual(0, sched.IsActive);
//        }

//        [TestMethod]
//        public async Task ReactivateSchedule()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var scheduleManager = scope.ServiceProvider.GetRequiredService<IScheduleManager>();
//                await scheduleManager.DeactivateScheduleAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            var sched = await GetScheduleAsync(1);
//            Assert.AreEqual(0, sched.IsActive);

//            scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var scheduleManager = scope.ServiceProvider.GetRequiredService<IScheduleManager>();
//                await scheduleManager.ReactivateScheduleAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            sched = await GetScheduleAsync(1);
//            Assert.AreEqual(1, sched.IsActive);
//        }

//        [TestMethod]
//        public async Task GetSchedulesToInsert()
//        {
//            for (int i = 0; i < 4; ++i)
//            {
//                await CreateWorkerAsync();
//                await CreateScheduleAsync(i + 1);
//            }
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var scheduleManager = scope.ServiceProvider.GetRequiredService<IScheduleManager>();
//                var schedules = await scheduleManager.GetSchedulesToInsertAsync(default);
//                Assert.AreEqual(4, schedules.Length);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task AddSchedule()
//        {
//            await CreateWorkerAsync();
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var scheduleManager = scope.ServiceProvider.GetRequiredService<IScheduleManager>();
//                await scheduleManager.AddScheduleAsync(1, true, true, true, true, false,
//                false, false, false, null, TimeSpan.FromHours(1), null, null, false, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            var schedule = await GetScheduleAsync(1);
//            Assert.AreEqual(1, schedule.ScheduleID);
//            Assert.AreEqual(1, schedule.Sunday);
//            Assert.AreEqual(0, schedule.Thursday);
//        }

//        [TestMethod]
//        public async Task UpdateSchedule()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var scheduleManager = scope.ServiceProvider.GetRequiredService<IScheduleManager>();
//                await scheduleManager.UpdateScheduleAsync(1, true, true, true, false, true, true, true,
//                    null, TimeSpan.FromMinutes(1), null, null, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        protected abstract DbConnection GetConnection();
//        protected abstract DbParameter Int64Parameter(string name, long? value);

//        private async Task CreateScheduleAsync(int workerID)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = $@"
//                INSERT INTO Schedules (
//                    WorkerID
//                    , Sunday
//                    , Monday
//                    , Tuesday
//                    , Wednesday
//                    , Thursday
//                    , Friday
//                    , Saturday
//                    , RecurTime
//                ) VALUES (
//                    {workerID}
//                    ,1, 1, 1, 1, 1, 1, 1
//                    ,010000000
//                );
//            ";
//            await comm.ExecuteNonQueryAsync();
//        }

//        private async Task CreateWorkerAsync()
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = $@"
//                INSERT INTO Workers (
//                    IsActive, WorkerName, DetailedDescription, EmailOnSuccess
//                    , ParentWorkerID, TimeoutMinutes, DirectoryName
//                    , Executable, ArgumentValues
//                ) VALUES (
//                    1, 'test worker {Guid.NewGuid():N}', '', ''
//                    , null, 20, 'test'
//                    ,'test', ''
//                );
//            ";
//            await comm.ExecuteNonQueryAsync();
//        }

//        private async Task<ScheduleEntity> GetScheduleAsync(int scheduleID)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = @"
//                SELECT * FROM Schedules WHERE ScheduleID = @ScheduleID;
//            ";
//            comm.Parameters.AddRange(new[]
//            {
//                Int64Parameter("@ScheduleID", scheduleID)
//            });
//            using var rdr = await comm.ExecuteReaderAsync();
//            await rdr.ReadAsync();
//            return Mapper.MapSchedule(rdr);
//        }
//    }
//}

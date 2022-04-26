//using System;
//using System.Data.Common;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SimpleSchedulerBusiness;
//using SimpleSchedulerData;
//using SimpleSchedulerEmail;
//using SimpleSchedulerEntities;
//using SimpleSchedulerModels.Exceptions;

//// TODO: Many more test cases covering different scenarios
//// TODO: Build better support for the yyyyMMddHHmmssfff and hhmmssfff constants so they can be converted back and forth more easily
//namespace SimpleSchedulerTests
//{
//    public abstract class JobManagerTestsBase
//    {
//        protected JobManagerTestsBase()
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
//        public async Task RestartStuckJobs()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "RUN");
//            await AssertJobStatusAsync(1, "RUN");

//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.RestartStuckJobsAsync(default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            await AssertJobStatusAsync(1, "NEW");
//        }

//        [TestMethod]
//        public async Task AcknowledgeError()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "ERR");
//            await AssertJobStatusAsync(1, "ERR");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.AcknowledgeErrorAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            await AssertJobStatusAsync(1, "ACK");
//        }

//        [TestMethod]
//        public async Task AddJob()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.AddJobAsync(1, DateTime.UtcNow, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            var job = await GetJobAsync(1);
//            Assert.AreEqual(1, job.JobID);
//            Assert.AreEqual(1, job.ScheduleID);
//            var queueDate = DateTime.ParseExact(job.QueueDateUTC.ToString(), "yyyyMMddHHmmssfff", null);
//            var insertDate = DateTime.ParseExact(job.InsertDateUTC.ToString(), "yyyyMMddHHmmssfff", null);
//            Assert.IsTrue(DateTime.UtcNow.Subtract(queueDate).TotalSeconds < 10);
//            Assert.IsTrue(DateTime.UtcNow.Subtract(insertDate).TotalSeconds < 10);
//            Assert.AreEqual("NEW", job.StatusCode);
//        }

//        [TestMethod]
//        public async Task GetJob()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                var job = await jobManager.GetJobAsync(1, default);
//                Assert.AreEqual(1, job.JobID);
//                Assert.AreEqual(1, job.ScheduleID);
//                var queueDate = job.QueueDateUTC;
//                var insertDate = job.InsertDateUTC;
//                Assert.IsTrue(DateTime.UtcNow.Subtract(queueDate).TotalSeconds < 10);
//                Assert.IsTrue(DateTime.UtcNow.Subtract(insertDate).TotalSeconds < 10);
//                Assert.AreEqual("NEW", job.StatusCode);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task CancelJob()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "NEW");
//            await AssertJobStatusAsync(1, "NEW");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CancelJobAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            await AssertJobStatusAsync(1, "CAN");
//        }

//        [TestMethod]
//        [ExpectedException(typeof(JobAlreadyRunningException))]
//        public async Task CancelJobRunning()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "RUN");
//            await AssertJobStatusAsync(1, "RUN");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CancelJobAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        [ExpectedException(typeof(JobAlreadyCompletedException))]
//        public async Task CancelJobSuccess()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "SUC");
//            await AssertJobStatusAsync(1, "SUC");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CancelJobAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        [ExpectedException(typeof(JobAlreadyCompletedException))]
//        public async Task CancelJobError()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "ERR");
//            await AssertJobStatusAsync(1, "ERR");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CancelJobAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        [ExpectedException(typeof(JobAlreadyCompletedException))]
//        public async Task CancelJobAcknowledged()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "ACK");
//            await AssertJobStatusAsync(1, "ACK");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CancelJobAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task CompleteJobSuccess()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "RUN");
//            await AssertJobStatusAsync(1, "RUN");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CompleteJobAsync(1, success: true, "abc", default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            var job = await GetJobAsync(1);
//            Assert.AreEqual("SUC", job.StatusCode);
//            Assert.AreEqual("abc", job.DetailedMessage);
//        }

//        [TestMethod]
//        public async Task CompleteJobFail()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "RUN");
//            await AssertJobStatusAsync(1, "RUN");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CompleteJobAsync(1, success: false, "abc", default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//            var job = await GetJobAsync(1);
//            Assert.AreEqual("ERR", job.StatusCode);
//            Assert.AreEqual("abc", job.DetailedMessage);
//        }

//        [TestMethod]
//        public async Task GetLatestJobs()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                var jobs = await jobManager.GetLatestJobsAsync(1, 10, null, null, false, default);
//                Assert.AreEqual(1, jobs.Length);
//                Assert.AreEqual(1, jobs[0].Job.JobID);
//                Assert.AreEqual(1, jobs[0].Schedule.ScheduleID);
//                Assert.AreEqual(1, jobs[0].Worker.WorkerID);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task GetOverdueJobs()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetJobStatusAsync(1, "RUN");
//            await CreateJobAsync(1);
//            await SetQueueDateAsync(2, DateTime.UtcNow.AddMinutes(-30));
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                var jobs = await jobManager.GetOverdueJobsAsync(default);
//                Assert.AreEqual(1, jobs.Length);
//                Assert.AreEqual(2, jobs[0].Job.JobID);
//                Assert.AreEqual(1, jobs[0].Schedule.ScheduleID);
//                Assert.AreEqual(1, jobs[0].Worker.WorkerID);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task GetLastQueuedJob()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await Task.Delay(10);
//            await CreateJobAsync(1);
//            await Task.Delay(10);
//            await CreateJobAsync(1);
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                var lastQueued = await jobManager.GetLastQueuedJobAsync(1, default);
//                Assert.AreEqual(3, lastQueued?.JobID);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task GetJobDetailedMessage()
//        {
//            await CreateWorkerAsync();
//            await CreateScheduleAsync(1);
//            await CreateJobAsync(1);
//            await SetDetailedMessageAsync(1, "asdf");
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                string? message = await jobManager.GetDetailedMessageAsync(1, default);
//                Assert.AreEqual("asdf", message);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task DequeueScheduledJobs()
//        {
//            for (int i = 0; i < 4; ++i)
//            {
//                await CreateWorkerAsync();
//                await CreateScheduleAsync(i + 1);
//                await CreateJobAsync(i + 1);
//                await Task.Delay(10);
//            }
//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                var dequeuedJobs = await jobManager.DequeueScheduledJobsAsync(default);
//                Assert.AreEqual(3, dequeuedJobs.Length);
//                Assert.IsTrue(new long[] { 1, 2, 3 }.SequenceEqual(dequeuedJobs.Select(x => x.Job.JobID)));
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }
//        }

//        [TestMethod]
//        public async Task ChildRunAfterParent()
//        {
//            await CreateWorkerAsync();
//            await CreateChildWorkerAsync(parentWorkerID: 1);
//            await CreateChildWorkerAsync(parentWorkerID: 2);

//            var scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var workerManager = scope.ServiceProvider.GetRequiredService<IWorkerManager>();
//                await workerManager.RunNowAsync(1, default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }

//            scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CompleteJobAsync(jobID: 1, success: true, detailedMessage: null, cancellationToken: default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }

//            // Ensure the first job is completed successfully, and the second job exists for schedule 2/worker 2
//            await AssertJobStatusAsync(jobID: 1, status: "SUC");
//            var job2 = await GetJobAsync(jobID: 2);
//            Assert.AreEqual("NEW", job2.StatusCode);
//            Assert.AreEqual(2, job2.ScheduleID);
//            var schedule2 = await GetScheduleAsync(2);
//            await AssertJobStatusAsync(jobID: 2, status: "NEW");
//            Assert.AreEqual(2, schedule2.WorkerID);

//            scope = GetServiceProvider().CreateScope();
//            try
//            {
//                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
//                await jobManager.CompleteJobAsync(jobID: 2, success: true, detailedMessage: null, cancellationToken: default);
//            }
//            finally
//            {
//                await ((IAsyncDisposable)scope).DisposeAsync();
//            }

//            // Ensure the second job is completed successfully, and the third job exists for schedule 3/worker 3
//            await AssertJobStatusAsync(jobID: 2, status: "SUC");
//            var job3 = await GetJobAsync(jobID: 3);
//            Assert.AreEqual("NEW", job3.StatusCode);
//            Assert.AreEqual(3, job3.ScheduleID);
//            var schedule3 = await GetScheduleAsync(3);
//            await AssertJobStatusAsync(jobID: 3, status: "NEW");
//            Assert.AreEqual(3, schedule3.WorkerID);
//        }

//        protected abstract DbConnection GetConnection();
//        protected abstract DbParameter Int64Parameter(string name, long? value);
//        protected abstract DbParameter StringParameter(string name, string? value, int? size = null);

//        private async Task CreateJobAsync(int scheduleID)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = $@"
//                INSERT INTO Jobs (
//                    ScheduleID 
//                    , InsertDateUTC
//                    , QueueDateUTC 
//                    , AcknowledgementID 
//                ) VALUES (
//                    {scheduleID}
//                    ,{DateTime.UtcNow:yyyyMMddHHmmssfff}
//                    ,{DateTime.UtcNow:yyyyMMddHHmmssfff}
//                    ,'{Guid.NewGuid():N}' 
//                );
//            ";
//            await comm.ExecuteNonQueryAsync();
//        }

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

//        private async Task CreateChildWorkerAsync(long parentWorkerID)
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
//                    1, 'child of {parentWorkerID} {Guid.NewGuid():N}', '', ''
//                    , {parentWorkerID}, 20, 'test'
//                    ,'test', ''
//                );
//            ";
//            await comm.ExecuteNonQueryAsync();
//        }

//        private async Task SetQueueDateAsync(long jobID, DateTime queueDateUTC)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = @"
//                UPDATE Jobs SET QueueDateUTC = @QueueDateUTC WHERE JobID = @JobID;
//            ";
//            comm.Parameters.AddRange(new[]
//            {
//                Int64Parameter("@QueueDateUTC", long.Parse(queueDateUTC.ToString("yyyyMMddHHmmssfff"))),
//                Int64Parameter("@JobID", jobID)
//            });
//            await comm.ExecuteNonQueryAsync();
//        }

//        private async Task SetDetailedMessageAsync(long jobID, string message)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = @"
//                UPDATE Jobs SET DetailedMessage = @DetailedMessage WHERE JobID = @JobID;
//            ";
//            comm.Parameters.AddRange(new[]
//            {
//                StringParameter("@DetailedMessage", message),
//                Int64Parameter("@JobID", jobID)
//            });
//            await comm.ExecuteNonQueryAsync();
//        }
//        private async Task SetJobStatusAsync(long jobID, string status)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = @"
//                UPDATE Jobs SET StatusCode = @StatusCode WHERE JobID = @JobID;
//            ";
//            comm.Parameters.AddRange(new[]
//            {
//                StringParameter("@StatusCode", status),
//                Int64Parameter("@JobID", jobID)
//            });
//            await comm.ExecuteNonQueryAsync();
//        }

//        private async Task AssertJobStatusAsync(int jobID, string status)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = @"
//                SELECT StatusCode FROM Jobs WHERE JobID = @JobID;
//            ";
//            comm.Parameters.AddRange(new[]
//            {
//                Int64Parameter("@JobID", jobID)
//            });
//            Assert.AreEqual(status, (string)(await comm.ExecuteScalarAsync())!);
//        }

//        private async Task<JobEntity> GetJobAsync(long jobID)
//        {
//            using var conn = GetConnection();
//            await conn.OpenAsync();
//            using var comm = conn.CreateCommand();
//            comm.CommandText = @"
//                SELECT * FROM Jobs WHERE JobID = @JobID;
//            ";
//            comm.Parameters.AddRange(new[]
//            {
//                Int64Parameter("@JobID", jobID)
//            });
//            using var rdr = await comm.ExecuteReaderAsync();
//            await rdr.ReadAsync();
//            return Mapper.MapJob(rdr);
//        }

//        private async Task<ScheduleEntity> GetScheduleAsync(long scheduleID)
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

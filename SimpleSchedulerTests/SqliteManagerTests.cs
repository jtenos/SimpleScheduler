using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Retry;
using SimpleSchedulerAppServices.Implementations.Sqlite;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerDomainModels;
using SimpleSchedulerEmail;

namespace SimpleSchedulerTests;

/// <summary>
/// End-to-end tests for the SQLite manager implementations against a real (temporary) SQLite file.
/// These exercise the SQL translations of the stored procedures. No SQL Server required.
/// </summary>
[TestClass]
public class SqliteManagerTests
{
    private string _dbDir = null!;
    private string _workerPath = null!;
    private IDatabase _db = null!;
    private CapturingEmailer _emailer = null!;
    private IWorkerManager _workers = null!;
    private IScheduleManager _schedules = null!;
    private IJobManager _jobs = null!;
    private IUserManager _users = null!;

    [TestInitialize]
    public void Init()
    {
        _dbDir = Path.Combine(Path.GetTempPath(), "ss_sqlite_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dbDir);

        // A valid worker executable (WorkerManager validates it exists on disk).
        _workerPath = Path.Combine(_dbDir, "workers");
        Directory.CreateDirectory(Path.Combine(_workerPath, "d"));
        File.WriteAllText(Path.Combine(_workerPath, "d", "run.sh"), "echo hi");

        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(_dbDir, "test.sqlite"),
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        AsyncRetryPolicy retry = Policy.Handle<Exception>().WaitAndRetryAsync(0, _ => TimeSpan.Zero);
        _db = new SqliteDatabase(connectionString, retry);
        _emailer = new CapturingEmailer();

        _workers = new WorkerManager(_db);
        _schedules = new ScheduleManager(_db);
        _jobs = new JobManager(NullLogger<JobManager>.Instance, _db, _emailer);
        _users = new UserManager(_db, _emailer, NullLogger<UserManager>.Instance);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_dbDir, recursive: true); } catch { /* best effort */ }
    }

    [TestMethod]
    public async Task Workers_Crud_And_Filters()
    {
        await _workers.AddWorkerAsync("Parent", "desc", "p@x.com", null, 20, "d", "run.sh", "", _workerPath);
        await _workers.AddWorkerAsync("Child", "desc", "", null, 20, "d", "run.sh", "", _workerPath);

        Worker[] all = await _workers.GetAllWorkersAsync();
        Assert.AreEqual(2, all.Length);

        long parentId = all.Single(w => w.WorkerName == "Parent").ID;
        long childId = all.Single(w => w.WorkerName == "Child").ID;

        Assert.AreEqual(1, (await _workers.GetAllWorkersAsync(workerName: "aren")).Length, "name LIKE filter");
        Assert.AreEqual(2, (await _workers.GetAllWorkersAsync(activeOnly: true)).Length, "activeOnly filter");
        Assert.AreEqual(2, (await _workers.GetWorkersAsync(new[] { parentId, childId })).Length, "GetWorkersAsync by ids");

        ApplicationException dup = await Assert.ThrowsExactlyAsync<ApplicationException>(
            () => _workers.AddWorkerAsync("Parent", "d", "", null, 20, "d", "run.sh", "", _workerPath));
        Assert.AreEqual("Name already exists", dup.Message);

        // Deactivate renames with an INACTIVE prefix; reactivate restores it.
        await _workers.DeactivateWorkerAsync(childId);
        Worker deactivated = await _workers.GetWorkerAsync(childId);
        Assert.IsFalse(deactivated.IsActive);
        StringAssert.StartsWith(deactivated.WorkerName, "INACTIVE:");

        await _workers.ReactivateWorkerAsync(childId);
        Worker reactivated = await _workers.GetWorkerAsync(childId);
        Assert.IsTrue(reactivated.IsActive);
        Assert.AreEqual("Child", reactivated.WorkerName);
    }

    [TestMethod]
    public async Task Workers_Update_DetectsCircularReference()
    {
        await _workers.AddWorkerAsync("A", "d", "", null, 20, "d", "run.sh", "", _workerPath);
        await _workers.AddWorkerAsync("B", "d", "", null, 20, "d", "run.sh", "", _workerPath);
        Worker[] all = await _workers.GetAllWorkersAsync();
        long aId = all.Single(w => w.WorkerName == "A").ID;
        long bId = all.Single(w => w.WorkerName == "B").ID;

        // B's parent = A (fine)
        await _workers.UpdateWorkerAsync(bId, "B", "d", "", aId, 20, "d", "run.sh", "", _workerPath);
        Assert.AreEqual(aId, (await _workers.GetWorkerAsync(bId)).ParentWorkerID);

        // A's parent = B would create a cycle
        ApplicationException ex = await Assert.ThrowsExactlyAsync<ApplicationException>(
            () => _workers.UpdateWorkerAsync(aId, "A", "d", "", bId, 20, "d", "run.sh", "", _workerPath));
        Assert.AreEqual("Circular reference", ex.Message);
    }

    [TestMethod]
    public async Task Schedules_Crud_And_Validation()
    {
        long workerId = await AddWorkerAsync("W");

        await _schedules.AddScheduleAsync(workerId, true, true, true, true, true, true, true,
            timeOfDayUTC: null, recurTime: new TimeSpan(0, 30, 0), recurBetweenStartUTC: null, recurBetweenEndUTC: null);

        Schedule[] schedules = await _schedules.GetAllSchedulesAsync();
        Assert.AreEqual(1, schedules.Length);
        Assert.AreEqual(new TimeSpan(0, 30, 0), schedules[0].RecurTime, "TimeSpan round-trips through TEXT");

        long schedId = schedules[0].ID;
        Assert.AreEqual(1, (await _schedules.GetSchedulesForWorkerAsync(workerId)).Length);
        Assert.AreEqual(schedId, (await _schedules.GetScheduleAsync(schedId)).ID);

        await Assert.ThrowsExactlyAsync<System.ComponentModel.DataAnnotations.ValidationException>(
            () => _schedules.AddScheduleAsync(workerId, false, false, false, false, false, false, false,
                new TimeSpan(1, 0, 0), null, null, null));

        await _schedules.DeactivateScheduleAsync(schedId);
        Assert.IsFalse((await _schedules.GetAllSchedulesAsync()).Any(s => s.ID == schedId));
    }

    [TestMethod]
    public async Task Jobs_FullLifecycle_QueueDequeueCompleteWithChildJobs()
    {
        long parentId = await AddWorkerAsync("Parent");
        long childId = await AddWorkerAsync("Child");
        await _workers.UpdateWorkerAsync(childId, "Child", "d", "", parentId, 20, "d", "run.sh", "", _workerPath);

        await _schedules.AddScheduleAsync(parentId, true, true, true, true, true, true, true,
            null, new TimeSpan(0, 30, 0), null, null);

        await _jobs.StartDueJobsAsync();
        JobWithWorkerID[] queued = await _jobs.GetLatestJobsAsync(1, 100, "NEW", null, null, false);
        Assert.AreEqual(1, queued.Length, "StartDueJobs queued a job");

        // Make it due in the past so dequeue picks it up.
        await ExecRawAsync("UPDATE Jobs SET QueueDateUTC='2000-01-01T00:00:00.0000000Z' WHERE StatusCode='NEW';");

        JobWithWorker[] dequeued = await _jobs.DequeueScheduledJobsAsync();
        Assert.AreEqual(1, dequeued.Length);
        Assert.AreEqual(parentId, dequeued[0].Worker.ID);
        long jobId = dequeued[0].ID;
        Assert.AreEqual("RUN", (await _jobs.GetJobAsync(jobId)).StatusCode);

        await _jobs.CompleteJobAsync(jobId, success: true, detailedMessage: "all good",
            adminEmail: "a@x.com", appUrl: "http://x", environmentName: "TEST", workerPath: _workerPath);

        Job done = await _jobs.GetJobAsync(jobId);
        Assert.AreEqual("SUC", done.StatusCode);
        Assert.IsNotNull(done.CompleteDateUTC);
        Assert.IsTrue(done.HasDetailedMessage);
        Assert.AreEqual("all good", await _jobs.GetDetailedMessageAsync(jobId, _workerPath));

        // Completing the parent successfully queues a one-time job for the active child worker.
        JobWithWorkerID[] childJobs = await _jobs.GetLatestJobsAsync(1, 100, "NEW", childId, null, false);
        Assert.AreEqual(1, childJobs.Length, "child one-time job created on parent success");
    }

    [TestMethod]
    public async Task Jobs_Cancel_And_Acknowledge()
    {
        long workerId = await AddWorkerAsync("W");
        await _schedules.AddScheduleAsync(workerId, true, true, true, true, true, true, true,
            null, new TimeSpan(0, 30, 0), null, null);
        await _jobs.StartDueJobsAsync();

        JobWithWorkerID newJob = (await _jobs.GetLatestJobsAsync(1, 100, "NEW", null, null, false)).Single();
        await _jobs.CancelJobAsync(newJob.ID);
        Assert.AreEqual("CAN", (await _jobs.GetJobAsync(newJob.ID)).StatusCode);

        // Move a fresh job to ERR, then acknowledge it.
        await _jobs.StartDueJobsAsync();
        await ExecRawAsync("UPDATE Jobs SET StatusCode='ERR' WHERE StatusCode='NEW';");
        JobWithWorkerID erroredRow = (await _jobs.GetLatestJobsAsync(1, 100, "ERR", null, null, false)).First();
        Job errored = await _jobs.GetJobAsync(erroredRow.ID);

        await _jobs.AcknowledgeErrorAsync(errored.AcknowledgementCode);
        Assert.AreEqual("ACK", (await _jobs.GetJobAsync(errored.ID)).StatusCode);
    }

    [TestMethod]
    public async Task Users_LoginSubmit_And_Validate()
    {
        await ExecRawAsync("INSERT INTO Users (EmailAddress) VALUES ('user@x.com');");

        Assert.AreEqual(1, (await _users.GetAllUserEmailsAsync(true)).Length);
        Assert.AreEqual(0, (await _users.GetAllUserEmailsAsync(false)).Length, "dropdown disabled returns empty");

        Assert.IsTrue(await _users.LoginSubmitAsync("user@x.com", "http://x"), "known user");
        Assert.IsFalse(await _users.LoginSubmitAsync("nobody@x.com", "http://x"), "unknown user");

        Guid code = _emailer.LastValidationCode;
        Assert.AreEqual("user@x.com", await _users.LoginValidateAsync(code, Guid.NewGuid()));

        // A used code is no longer valid.
        ApplicationException reused = await Assert.ThrowsExactlyAsync<ApplicationException>(
            () => _users.LoginValidateAsync(code, Guid.NewGuid()));
        Assert.AreEqual("Not found", reused.Message);
    }

    private async Task<long> AddWorkerAsync(string name)
    {
        await _workers.AddWorkerAsync(name, "d", "", null, 20, "d", "run.sh", "", _workerPath);
        return (await _workers.GetAllWorkersAsync(workerName: name)).Single(w => w.WorkerName == name).ID;
    }

    private async Task ExecRawAsync(string sql)
    {
        using SqliteConnection conn = new(new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(_dbDir, "test.sqlite"),
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString());
        await conn.OpenAsync();
        // Make sure the schema exists even when raw SQL runs before any manager call.
        await SimpleSchedulerSqliteDB.SqliteSchemaInitializer.EnsureSchemaAsync(conn);
        using SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    private sealed class CapturingEmailer : IEmailer
    {
        public Guid LastValidationCode { get; private set; }

        public void SendEmailToAdmin(string subject, string bodyHTML) { }
        public Task SendEmailToAdminAsync(string subject, string bodyHTML) => Task.CompletedTask;
        public void SendEmail(string[] toAddresses, string subject, string bodyHTML) { }

        public Task SendEmailAsync(string[] toAddresses, string subject, string bodyHTML)
        {
            const string marker = "validate-user/";
            int idx = bodyHTML.IndexOf(marker, StringComparison.Ordinal);
            if (idx >= 0 && Guid.TryParse(bodyHTML.Substring(idx + marker.Length, 36), out Guid g))
            {
                LastValidationCode = g;
            }
            return Task.CompletedTask;
        }
    }
}

using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public class SqliteDatabase
        : BaseDatabase
    {
        public SqliteDatabase(IConfiguration config)
            : base(config) { }

        protected override DbConnection GetConnection()
            => new SqliteConnection
            {
                ConnectionString = Config.GetConnectionString("SimpleScheduler")
            };

        public override DbParameter GetInt64Parameter(string name, long? value)
            => new SqliteParameter(name, SqliteType.Integer) { Value = value ?? (object)DBNull.Value };

        public override DbParameter GetStringParameter(string name, string? value, bool isFixed, int size)
            => new SqliteParameter(name, SqliteType.Text) { Value = value ?? (object)DBNull.Value };

        public override DbParameter GetBinaryParameter(string name, byte[]? value, bool isFixed, int size)
            => new SqliteParameter(name, SqliteType.Blob) { Value = value ?? (object)DBNull.Value };

        public override string GetLastAutoIncrementQuery => "SELECT last_insert_rowid();";

        public override string GetOffsetLimitClause(int offset, int limit)
            => $" LIMIT {limit} OFFSET {offset} ";

        public static async Task CreateDatabaseAsync(string databaseFileName)
        {
            Console.WriteLine($"Creating database for file |{databaseFileName}|");
            using var conn = new SqliteConnection($"Data Source={databaseFileName};");
            await conn.OpenAsync().ConfigureAwait(false);
            using var comm = conn.CreateCommand();
            comm.CommandText = DATABASE_CREATION_SCRIPT;
            await comm.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private const string DATABASE_CREATION_SCRIPT = @"
            CREATE TABLE Users
            (
                EmailAddress TEXT NOT NULL
            );

            CREATE UNIQUE INDEX IX_Users_EmailAddress
            ON Users(EmailAddress);

            --------------------------

            CREATE TABLE LoginAttempts
            (
                LoginAttemptID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                , EmailAddress TEXT NOT NULL

                -- Dates are YYYYMMDDHHMMSSFFF
                ,SubmitDateUTC INTEGER NOT NULL

                -- The unique key that is sent to their email address to log in
                ,ValidationKey TEXT NOT NULL

                -- When the user clicks their email link and gets logged in.
                , ValidationDateUTC INTEGER NULL
            );

            CREATE UNIQUE INDEX IX_LoginAttempts_ValidationKey
            ON LoginAttempts(ValidationKey);

            --------------------------

            CREATE TABLE Workers
            (
                WorkerID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                , IsActive INTEGER NOT NULL DEFAULT 1
                , WorkerName TEXT NOT NULL
                , DetailedDescription TEXT NOT NULL
                , EmailOnSuccess TEXT NOT NULL
                , ParentWorkerID INTEGER NULL
                , TimeoutMinutes INTEGER NOT NULL
                , DirectoryName TEXT NOT NULL
                , Executable TEXT NOT NULL
                , ArgumentValues TEXT NOT NULL

                , FOREIGN KEY (ParentWorkerID) REFERENCES Workers (WorkerID)
            );

            CREATE UNIQUE INDEX IX_Workers_WorkerName
            ON Workers(WorkerName);

            --------------------------

            CREATE TABLE Schedules
            (
                ScheduleID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                , IsActive INTEGER NOT NULL DEFAULT 1

                , WorkerID INTEGER NOT NULL

                , Sunday INTEGER NOT NULL
                , Monday INTEGER NOT NULL
                , Tuesday INTEGER NOT NULL
                , Wednesday INTEGER NOT NULL
                , Thursday INTEGER NOT NULL
                , Friday INTEGER NOT NULL
                , Saturday INTEGER NOT NULL

                -- Times are HHMMSSFFF
                , TimeOfDayUTC INTEGER NULL
                , RecurTime INTEGER NULL
                , RecurBetweenStartUTC INTEGER NULL
                , RecurBetweenEndUTC INTEGER NULL

                , OneTime INTEGER NOT NULL DEFAULT 0

                , CHECK (TimeOfDayUTC IS NOT NULL OR RecurTime IS NOT NULL)
                ,CHECK(RecurBetweenStartUTC IS NULL OR RecurTime IS NOT NULL)
                ,CHECK(RecurBetweenEndUTC IS NULL OR RecurTime IS NOT NULL)
                ,FOREIGN KEY(WorkerID) REFERENCES Workers(WorkerID)
            );

            CREATE INDEX IX_Schedules_WorkerID ON Schedules(WorkerID);

            --------------------------

            CREATE TABLE Jobs
            (
                JobID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                , ScheduleID INTEGER NOT NULL

                -- Dates are YYYYMMDDHHMMSSFFF
                , InsertDateUTC INTEGER NOT NULL
                , QueueDateUTC INTEGER NOT NULL
                , CompleteDateUTC INTEGER NULL

                , StatusCode TEXT NOT NULL DEFAULT 'NEW'

                , DetailedMessage TEXT NULL

                -- If this job errors, this is the ID to acknowledge the error
                -- If the job does not error, this is ignored
                , AcknowledgementID TEXT NOT NULL
                , AcknowledgementDate BIGINT NULL
                
                , DetailedMessageSize INTEGER NOT NULL DEFAULT 0

                , FOREIGN KEY ([ScheduleID]) REFERENCES [Schedules] ([ScheduleID])
                , CHECK (
                    [StatusCode] = 'NEW' OR[StatusCode] = 'CAN' OR[StatusCode] = 'ERR'
                    OR[StatusCode] = 'RUN' OR[StatusCode] = 'ACK' OR[StatusCode] = 'SUC'
                )
            );
            CREATE INDEX IX_Jobs_Status ON Jobs(StatusCode);
            CREATE INDEX IX_Jobs_ScheduleID ON Jobs(ScheduleID);
            CREATE INDEX IX_Jobs_AcknowledgementID ON Jobs(AcknowledgementID);
            CREATE INDEX IX_QueueDateSchComplStatus ON Jobs (QueueDateUTC, ScheduleID, CompleteDateUTC, StatusCode);

            --------------------------

            CREATE TABLE JobsArchive
            (
                JobArchiveID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                ,JobID INTEGER NOT NULL
                ,ScheduleID INTEGER NOT NULL
                ,InsertDateUTC INTEGER NOT NULL
                ,QueueDateUTC INTEGER NOT NULL
                ,CompleteDateUTC INTEGER NULL
                ,StatusCode TEXT NOT NULL
                ,DetailedMessage BLOB NULL -- Brotli-compressed
                ,AcknowledgementID TEXT NOT NULL
                ,AcknowledgementDate BIGINT NULL
                ,DetailedMessageSize INTEGER NOT NULL
            );

            --------------------------
        ";
    }
}

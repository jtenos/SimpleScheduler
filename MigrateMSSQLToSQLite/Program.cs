using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

const string OLD_CS = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SimpleSchedulerSqlServerDB;TrustServerCertificate=True;Integrated Security=SSPI;";
const string NEW_CS = @"Data Source=C:\Users\jtenos\source\repos\SimpleScheduler\data\scheduler.sqlite3";

static object FormatDateTime(DateTime? dateTime)
{
	return dateTime?.ToString("yyyy\\-MM\\-dd\\THH\\:mm\\:ss\\.fff")
		?? (object)DBNull.Value;
}

static object FormatTime(TimeSpan? dateTime)
{
	return dateTime?.ToString("hh\\:mm\\:ss\\.fff")
		?? (object)DBNull.Value;
}

using SqlConnection oldConn = new(OLD_CS);
oldConn.Open();

using SqliteConnection newConn = new(NEW_CS);
newConn.Open();

using (SqliteCommand newComm = newConn.CreateCommand())
{
	newComm.CommandText = @"
		CREATE TABLE IF NOT EXISTS [users] (
			 [email_address] TEXT NOT NULL PRIMARY KEY
		);

		CREATE TABLE IF NOT EXISTS [login_attempts] (
			 [id] INTEGER NOT NULL PRIMARY KEY
			,[submit_date_utc] TEXT NOT NULL
			,[email_address] TEXT NOT NULL
			,[validation_code] TEXT NOT NULL
			,[validate_date_utc] TEXT NULL
		);

		CREATE UNIQUE INDEX IF NOT EXISTS [ix_login_attempts_validation_code]
		ON [login_attempts] ([validation_code]);

		CREATE TABLE IF NOT EXISTS [workers] (
			 [id] INTEGER NOT NULL PRIMARY KEY
			,[is_active] NOT NULL DEFAULT 1
			,[worker_name] TEXT NOT NULL UNIQUE
			,[detailed_description] TEXT NOT NULL
			,[email_on_success] TEXT NOT NULL
			,[parent_worker_id] INTEGER NULL
			,[timeout_minutes] INTEGER NOT NULL
			,[directory_name] TEXT NOT NULL
			,[executable] TEXT NOT NULL
			,[argument_values] TEXT NOT NULL
		);

		CREATE TABLE IF NOT EXISTS [schedules] (
			 [id] INTEGER NOT NULL PRIMARY KEY
			,[is_active] INTEGER NOT NULL DEFAULT 1
			,[worker_id] INTEGER NOT NULL
			,[sunday] INTEGER NOT NULL
			,[monday] INTEGER NOT NULL
			,[tuesday] INTEGER NOT NULL
			,[wednesday] INTEGER NOT NULL
			,[thursday] INTEGER NOT NULL
			,[friday] INTEGER NOT NULL
			,[saturday] INTEGER NOT NULL
			,[time_of_day_utc] TEXT NULL
			,[recur_time] TEXT NULL
			,[recur_between_start_utc] TEXT NULL
			,[recur_between_end_utc] TEXT NULL
			,[one_time] INTEGER NOT NULL DEFAULT 0
		);

		CREATE INDEX IF NOT EXISTS [ix_schedules_workerid]
		ON [schedules] ([worker_id]);

		CREATE TABLE IF NOT EXISTS [jobs] (
			 [id] INTEGER NOT NULL PRIMARY KEY
			,[schedule_id] INTEGER NOT NULL
			,[insert_date_utc] TEXT NOT NULL
			,[queue_date_utc] TEXT NOT NULL
			,[complete_date_utc] TEXT NULL
			,[status_code] TEXT NOT NULL
			,[acknowledgement_code] TEXT NOT NULL
			,[acknowledgement_date] TEXT NULL
			,[has_detailed_message] INTEGER NOT NULL DEFAULT 0
		);

		CREATE INDEX IF NOT EXISTS [ix_jobs_schedule_id]
		ON [jobs] (
			[schedule_id], [insert_date_utc], [queue_date_utc], [complete_date_utc],
			[status_code], [acknowledgement_code], [has_detailed_message]
		);

		CREATE INDEX IF NOT EXISTS [ix_jobs_status_code]
		ON [jobs] (
			[status_code], [schedule_id], [insert_date_utc], [queue_date_utc],
			[complete_date_utc], [acknowledgement_code], [has_detailed_message]
		);

		CREATE INDEX IF NOT EXISTS [ix_jobs_acknowledgement_code]
		ON [jobs] ([acknowledgement_code]);
	";
	newComm.ExecuteNonQuery();
}

using (SqliteTransaction tran = newConn.BeginTransaction())
{
	using SqlCommand oldComm = oldConn.CreateCommand();
	oldComm.CommandText = @"SELECT * FROM [app].[Users];";
	using SqlDataReader rdr = oldComm.ExecuteReader();
	while (rdr.Read())
	{
		using SqliteCommand newComm = newConn.CreateCommand();
		newComm.Transaction = tran;
		newComm.CommandText = @"INSERT INTO [users] ([email_address])
								VALUES (@email_address);";
		newComm.Parameters.AddWithValue("@email_address", rdr["EmailAddress"]);
		newComm.ExecuteNonQuery();
	}
	tran.Commit();
}

using (SqliteTransaction tran = newConn.BeginTransaction())
{
	using SqlCommand oldComm = oldConn.CreateCommand();
	oldComm.CommandText = @"SELECT * FROM [app].[LoginAttempts];";
	using SqlDataReader rdr = oldComm.ExecuteReader();
	while (rdr.Read())
	{
		using SqliteCommand newComm = newConn.CreateCommand();
		newComm.Transaction = tran;
		newComm.CommandText = @"INSERT INTO [login_attempts] (
			 [id],[submit_date_utc],[email_address],[validation_code],[validate_date_utc]
		) VALUES (
			@id,@submit_date_utc,@email_address,@validation_code,@validate_date_utc
		);";
		newComm.Parameters.AddWithValue("@id", rdr["ID"]);
		newComm.Parameters.AddWithValue("@submit_date_utc", FormatDateTime((DateTime)rdr["SubmitDateUTC"]));
		newComm.Parameters.AddWithValue("@email_address", rdr["EmailAddress"]);
		newComm.Parameters.AddWithValue("@validation_code", ((Guid)rdr["ValidationCode"]).ToString("N"));
		newComm.Parameters.AddWithValue("@validate_date_utc", FormatDateTime(rdr["ValidateDateUTC"] as DateTime?));
		newComm.ExecuteNonQuery();
	}
	tran.Commit();
}

using (SqliteTransaction tran = newConn.BeginTransaction())
{
	using SqlCommand oldComm = oldConn.CreateCommand();
	oldComm.CommandText = @"SELECT * FROM [app].[Workers];";
	using SqlDataReader rdr = oldComm.ExecuteReader();
	while (rdr.Read())
	{
		using SqliteCommand newComm = newConn.CreateCommand();
		newComm.Transaction = tran;
		newComm.CommandText = @"INSERT INTO [workers] (
			[id],[is_active],[worker_name],[detailed_description],[email_on_success],
			[parent_worker_id],[timeout_minutes],[directory_name],[executable],[argument_values]
		) VALUES (
			@id,@is_active,@worker_name,@detailed_description,@email_on_success,
			@parent_worker_id,@timeout_minutes,@directory_name,@executable,@argument_values
		);";
		newComm.Parameters.AddWithValue("@id", rdr["ID"]);
		newComm.Parameters.AddWithValue("@is_active", (bool)rdr["IsActive"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@worker_name", rdr["WorkerName"]);
		newComm.Parameters.AddWithValue("@detailed_description", rdr["DetailedDescription"]);
		newComm.Parameters.AddWithValue("@email_on_success", rdr["EmailOnSuccess"]);
		newComm.Parameters.AddWithValue("@parent_worker_id", rdr["ParentWorkerID"] as long? ?? (object)DBNull.Value);
		newComm.Parameters.AddWithValue("@timeout_minutes", rdr["TimeoutMinutes"]);
		newComm.Parameters.AddWithValue("@directory_name", rdr["DirectoryName"]);
		newComm.Parameters.AddWithValue("@executable", rdr["Executable"]);
		newComm.Parameters.AddWithValue("@argument_values", rdr["ArgumentValues"]);
		newComm.ExecuteNonQuery();
	}
	tran.Commit();
}

using (SqliteTransaction tran = newConn.BeginTransaction())
{
	using SqlCommand oldComm = oldConn.CreateCommand();
	oldComm.CommandText = @"SELECT * FROM [app].[Schedules];";
	using SqlDataReader rdr = oldComm.ExecuteReader();
	while (rdr.Read())
	{
		using SqliteCommand newComm = newConn.CreateCommand();
		newComm.Transaction = tran;
		newComm.CommandText = @"INSERT INTO [schedules] (
			[id],[is_active],[worker_id],[sunday],[monday],[tuesday],[wednesday],
			[thursday],[friday],[saturday],[time_of_day_utc],[recur_time],
			[recur_between_start_utc],[recur_between_end_utc],[one_time]
		) VALUES (
			@id,@is_active,@worker_id,@sunday,@monday,@tuesday,@wednesday,
			@thursday,@friday,@saturday,@time_of_day_utc,@recur_time,
			@recur_between_start_utc,@recur_between_end_utc,@one_time
		);";
		newComm.Parameters.AddWithValue("@id", rdr["ID"]);
		newComm.Parameters.AddWithValue("@is_active", (bool)rdr["IsActive"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@worker_id", rdr["WorkerID"]);
		newComm.Parameters.AddWithValue("@sunday", (bool)rdr["Sunday"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@monday", (bool)rdr["Monday"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@tuesday", (bool)rdr["Tuesday"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@wednesday", (bool)rdr["Wednesday"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@thursday", (bool)rdr["Thursday"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@friday", (bool)rdr["Friday"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@saturday", (bool)rdr["Saturday"] == true ? 1 : 0);
		newComm.Parameters.AddWithValue("@time_of_day_utc", FormatTime(rdr["TimeOfDayUTC"] as TimeSpan?));
		newComm.Parameters.AddWithValue("@recur_time", FormatTime(rdr["RecurTime"] as TimeSpan?));
		newComm.Parameters.AddWithValue("@recur_between_start_utc", FormatTime(rdr["RecurBetweenStartUTC"] as TimeSpan?));
		newComm.Parameters.AddWithValue("@recur_between_end_utc", FormatTime(rdr["RecurBetweenEndUTC"] as TimeSpan?));
		newComm.Parameters.AddWithValue("@one_time", (bool)rdr["OneTime"] == true ? 1 : 0);
		newComm.ExecuteNonQuery();
	}
	tran.Commit();
}

using (SqliteTransaction tran = newConn.BeginTransaction())
{
	using SqlCommand oldComm = oldConn.CreateCommand();
	oldComm.CommandText = @"SELECT * FROM [app].[Jobs];";
	using SqlDataReader rdr = oldComm.ExecuteReader();
	while (rdr.Read())
	{
		using SqliteCommand newComm = newConn.CreateCommand();
		newComm.Transaction = tran;
		newComm.CommandText = @"INSERT INTO [jobs] (
			[id],[schedule_id],[insert_date_utc],[queue_date_utc],[complete_date_utc],
			[status_code],[acknowledgement_code],[acknowledgement_date],[has_detailed_message]
		) VALUES (
			@id,@schedule_id,@insert_date_utc,@queue_date_utc,@complete_date_utc,
			@status_code,@acknowledgement_code,@acknowledgement_date,@has_detailed_message
		);";
		newComm.Parameters.AddWithValue("@id", rdr["ID"]);
		newComm.Parameters.AddWithValue("@schedule_id", rdr["ScheduleID"]);
		newComm.Parameters.AddWithValue("@insert_date_utc", FormatDateTime((DateTime)rdr["InsertDateUTC"]));
		newComm.Parameters.AddWithValue("@queue_date_utc", FormatDateTime((DateTime)rdr["QueueDateUTC"]));
		newComm.Parameters.AddWithValue("@complete_date_utc", FormatDateTime(rdr["CompleteDateUTC"] as DateTime?));
		newComm.Parameters.AddWithValue("@status_code", rdr["StatusCode"]);
		newComm.Parameters.AddWithValue("@acknowledgement_code", ((Guid)rdr["AcknowledgementCode"]).ToString("N"));
		newComm.Parameters.AddWithValue("@acknowledgement_date", FormatDateTime(rdr["AcknowledgementDate"] as DateTime?));
		newComm.Parameters.AddWithValue("@has_detailed_message", (bool)rdr["HasDetailedMessage"] == true ? 1 : 0);
		newComm.ExecuteNonQuery();
	}
	tran.Commit();
}

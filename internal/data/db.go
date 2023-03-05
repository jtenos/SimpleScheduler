package data

import (
	"context"
	"database/sql"
	"fmt"

	_ "github.com/mattn/go-sqlite3"
	"jtenos.com/simplescheduler/internal/ctxutil"
)

func InitDB(ctx context.Context) error {

	db, err := open(ctx)
	if err != nil {
		return err
	}
	tx, err := db.BeginTx(ctx, &sql.TxOptions{})
	if err != nil {
		return err
	}

	tx.ExecContext(ctx, `
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

		CREATE TABLE [jobs] (
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
	`)

	tx.Commit()

	return nil
}

func open(ctx context.Context) (*sql.DB, error) {

	var err error
	var db *sql.DB

	if db, err = sql.Open("sqlite3", ctxutil.GetDBFileName(ctx)); err != nil {
		return nil, fmt.Errorf("opening database: %w", err)
	}

	if _, err = db.Exec("PRAGMA journal_mode = wal;"); err != nil {
		return nil, fmt.Errorf("enable wal: %w", err)
	}

	return db, nil
}

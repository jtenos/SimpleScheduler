package sqlite

import (
	"context"
	"database/sql"
	"fmt"
	"os"
	"path/filepath"
)

type DB struct {
	db         *sql.DB
	ctx        context.Context
	cancel     func()
	dbFileName string
}

func NewDB(ctx context.Context, dbFileName string) *DB {
	db := &DB{
		dbFileName: dbFileName,
	}
	db.ctx, db.cancel = context.WithCancel(ctx)
	return db
}

func (db *DB) Init(ctx context.Context) error {
	db.Open()
	tx, err := db.BeginTran(ctx)
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

func (db *DB) Open() error {

	var err error

	if db.dbFileName == "" {
		return fmt.Errorf("dbFileName required")
	}

	if db.dbFileName != ":memory:" {
		if err = os.MkdirAll(filepath.Dir(db.dbFileName), 0700); err != nil {
			return fmt.Errorf("creating directory for %s, %w", db.dbFileName, err)
		}
	}

	if db.db, err = sql.Open("sqlite3", db.dbFileName); err != nil {
		return fmt.Errorf("opening database: %w", err)
	}

	if _, err = db.db.Exec("PRAGMA journal_mode = wal;"); err != nil {
		return fmt.Errorf("enable wal: %w", err)
	}

	return nil
}

func (db *DB) Close() error {
	db.cancel()
	if db.db != nil {
		return db.db.Close()
	}
	return nil
}

func (db *DB) BeginTran(ctx context.Context) (*sql.Tx, error) {
	return db.db.BeginTx(ctx, &sql.TxOptions{})
}

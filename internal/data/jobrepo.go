package data

import (
	"context"
	"database/sql"
	"time"

	"github.com/jtenos/simplescheduler/internal/util"
)

type JobRepo struct{ ctx context.Context }

func NewJobRepo(ctx context.Context) *JobRepo {
	return &JobRepo{ctx}
}

func (repo *JobRepo) CreateJob(scheduleID int64, queueDateUTC time.Time) (jobID int64, err error) {
	db, err := open(repo.ctx)
	if err != nil {
		return
	}
	defer db.Close()

	query := `
		INSERT INTO [jobs] (
			 [schedule_id], [insert_date_utc], [queue_date_utc], [acknowledgement_code]
		) VALUES (
			 @schedule_id, @insert_date_utc, @queue_date_utc, @acknowledgement_code
		);
	
		SELECT last_insert_rowid();
	`

	row := db.QueryRowContext(repo.ctx, query,
		sql.Named("schedule_id", scheduleID),
		sql.Named("insert_date_utc", time.Now().UTC().Format(time.RFC3339)),
		sql.Named("queue_date_utc", queueDateUTC.Format(time.RFC3339)),
		sql.Named("acknowledgement_code", util.UuidLower()),
	)

	err = row.Scan(&jobID)

	return
}

// Cancel a job only if it hasn't already started
func (repo *JobRepo) CancelJob(jobID int64) error {
	db, err := open(repo.ctx)
	if err != nil {
		return err
	}
	defer db.Close()

	query := `
		UPDATE [jobs]
		SET [status_code] = 'CAN'
		WHERE [id] = @id
		AND [status_code] = 'NEW';
	`

	_, err = db.ExecContext(repo.ctx, query, sql.Named("id", jobID))
	return err
}

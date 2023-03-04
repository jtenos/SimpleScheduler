package datamodels

import "database/sql"

// [workers] table
type Worker struct {
	ID                  int64  `db:"id"`
	IsActive            bool   `db:"is_active"`
	WorkerName          string `db:"worker_name"`
	DetailedDescription string `db:"detailed_description"`
	EmailOnSuccess      string `db:"email_on_success"`
	ParentWorkerID      *int64 `db:"parent_worker_id"`
	TimeoutMinutes      int32  `db:"timeout_minutes"`
	DirectoryName       string `db:"directory_name"`
	Executable          string `db:"executable"`
	ArgumentValues      string `db:"argument_values"`
}

func (w *Worker) Hydrate(rows *sql.Rows) error {
	return rows.Scan(&w.ID, &w.IsActive, &w.WorkerName, &w.DetailedDescription, &w.EmailOnSuccess,
		&w.ParentWorkerID, &w.TimeoutMinutes, &w.DirectoryName, &w.Executable, &w.ArgumentValues)
}

package entity

import "database/sql"

type UserEntity struct {
	EmailAddress string `db:"email_address"`
}

func (u *UserEntity) Hydrate(rows *sql.Rows) error {
	return rows.Scan(&u.EmailAddress)
}

type LoginAttemptEntity struct {
	ID              int64  `db:"id"`
	SubmitDateUTC   string `db:"submit_date_utc"`
	EmailAddress    string `db:"email_address"`
	ValidationCode  string `db:"validation_code"`
	ValidateDateUTC string `db:"validate_date_utc"`
}

type WorkerEntity struct {
	ID                  int64  `db:"id"`
	IsActive            int64  `db:"is_active"`
	WorkerName          string `db:"worker_name"`
	DetailedDescription string `db:"detailed_description"`
	EmailOnSuccess      string `db:"email_on_success"`
	ParentWorkerID      *int64 `db:"parent_worker_id"`
	TimeoutMinutes      int64  `db:"timeout_minutes"`
	DirectoryName       string `db:"directory_name"`
	Executable          string `db:"executable"`
	ArgumentValues      string `db:"argument_values"`
}

func (w *WorkerEntity) Hydrate(rows *sql.Rows) error {
	return rows.Scan(&w.ID, &w.IsActive, &w.WorkerName, &w.DetailedDescription, &w.EmailOnSuccess,
		&w.ParentWorkerID, &w.TimeoutMinutes, &w.DirectoryName, &w.Executable, &w.ArgumentValues)
}

type ScheduleEntity struct {
	ID                   int64  `db:"id"`
	IsActive             int64  `db:"is_active"`
	WorkerID             int64  `db:"worker_id"`
	Sunday               int64  `db:"sunday"`
	Monday               int64  `db:"monday"`
	Tuesday              int64  `db:"tuesday"`
	Wednesday            int64  `db:"wednesday"`
	Thursday             int64  `db:"thursday"`
	Friday               int64  `db:"friday"`
	Saturday             int64  `db:"saturday"`
	TimeOfDayUTC         string `db:"time_of_day_utc"`
	RecurTime            string `db:"recur_time"`
	RecurBetweenStartUTC string `db:"recur_between_start_utc"`
	RecurBetweenEndUTC   string `db:"recur_between_end_utc"`
	OneTime              int64  `db:"one_time"`
}

type JobEntity struct {
	ID                  int64  `db:"id"`
	ScheduleID          int64  `db:"schedule_id"`
	InsertDateUTC       string `db:"insert_date_utc"`
	QueueDateUTC        string `db:"queue_date_utc"`
	CompleteDateUTC     string `db:"complete_date_utc"`
	StatusCode          string `db:"status_code"`
	AcknowledgementCode string `db:"acknowledgement_code"`
	AcknowledgementDate string `db:"acknowledgement_date"`
	HasDetailedMessage  int64  `db:"has_detailed_message"`
}

package datamodels

// [app].[Workers] table
type Worker struct {
	ID                  int64  `db:"ID"`
	IsActive            bool   `db:"IsActive"`
	WorkerName          string `db:"WorkerName"`
	DetailedDescription string `db:"DetailedDescription"`
	EmailOnSuccess      string `db:"EmailOnSuccess"`
	ParentWorkerID      *int64 `db:"ParentWorkerID"`
	TimeoutMinutes      int32  `db:"TimeoutMinutes"`
	DirectoryName       string `db:"DirectoryName"`
	Executable          string `db:"Executable"`
	ArgumentValues      string `db:"ArgumentValues"`
}

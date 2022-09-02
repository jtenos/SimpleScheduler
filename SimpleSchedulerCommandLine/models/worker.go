package models

type Worker struct {
	ID                  int64  `json:"id"`
	IsActive            bool   `json:"isActive"`
	WorkerName          string `json:"workerName"`
	DetailedDescription string `json:"detailedDescription"`
	EmailOnSuccess      string `json:"emailOnSuccess"`
	ParentWorkerID      int64  `json:"parentWorkerID"`
	TimeoutMinutes      int32  `json:"timeoutMinutes"`
	DirectoryName       string `json:"directoryName"`
	Executable          string `json:"executable"`
	ArgumentValues      string `json:"argumentValues"`
}

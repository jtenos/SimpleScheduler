package apimodels

type UpdateWorkerRequest struct {
	ID                  int64  `json:"id"`
	WorkerName          string `json:"workerName"`
	DetailedDescription string `json:"detailedDescription"`
	EmailOnSuccess      string `json:"emailOnSuccess"`
	ParentWorkerID      *int64 `json:"parentWorkerID"`
	TimeoutMinutes      int32  `json:"timeoutMinutes"`
	DirectoryName       string `json:"directoryName"`
	Executable          string `json:"executable"`
	ArgumentValues      string `json:"argumentValues"`
}

func NewUpdateWorkerRequest(id int64, workerName string, detailedDescription string, emailOnSuccess string,
	parentWorkerID *int64, timeoutMinutes int32, directoryName string, executable string,
	argumentValues string) *UpdateWorkerRequest {
	return &UpdateWorkerRequest{
		ID:                  id,
		WorkerName:          workerName,
		DetailedDescription: detailedDescription,
		EmailOnSuccess:      emailOnSuccess,
		ParentWorkerID:      parentWorkerID,
		TimeoutMinutes:      timeoutMinutes,
		DirectoryName:       directoryName,
		Executable:          executable,
		ArgumentValues:      argumentValues,
	}
}

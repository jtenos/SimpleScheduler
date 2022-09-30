package models

type JobWithWorkerID struct {
	ID                  int64       `json:"id"`
	ScheduleID          int64       `json:"scheduleID"`
	InsertDateUTC       *CustomTime `json:"insertDateUTC"`
	QueueDateUTC        *CustomTime `json:"queueDateUTC"`
	CompleteDateUTC     *CustomTime `json:"completeDateUTC"`
	StatusCode          string      `json:"statusCode"`
	AcknowledgementCode string      `json:"acknowledgementCode"`
	AcknowledgementDate *CustomTime `json:"acknowledgementDate"`
	HasDetailedMessage  bool        `json:"hasDetailedMessage"`
	FriendlyDuration    string      `json:"friendlyDuration"`
	WorkerID            int64       `json:"workerID"`
	WorkerName          string      `json:"workerName"`
}

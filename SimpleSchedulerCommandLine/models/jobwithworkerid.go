package models

import "time"

type JobWithWorkerID struct {
	ID                  int64
	ScheduleID          int64
	InsertDateUTC       time.Time
	QueueDateUTC        time.Time
	CompleteDateUTC     time.Time
	StatusCode          string
	AcknowledgementCode string
	AcknowledgementDate time.Time
	HasDetailedMessage  bool
	FriendlyDuration    string
	WorkerID            int64
	WorkerName          string
}

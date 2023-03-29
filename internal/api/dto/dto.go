package dto

type Time struct {
	Hour   int64 `json:"hour"`
	Minute int64 `json:"minute"`
}

type DateTime struct {
	Time
	Year  int64 `json:"year"`
	Month int64 `json:"month"`
	Day   int64 `json:"day"`

	Second      int64 `json:"second"`
	Millisecond int64 `json:"millisecond"`
}

type JobDTO struct {
	ID                  int64     `json:"id"`
	ScheduleID          int64     `json:"scheduleID"`
	InsertDateUTC       DateTime  `json:"insertDateUTC"`
	QueueDateUTC        DateTime  `json:"queueDateUTC"`
	CompleteDateUTC     *DateTime `json:"completeDateUTC"`
	StatusCode          string    `json:"statusCode"`
	AcknowledgementCode string    `json:"acknowledgementCode"`
	AcknowledgementDate *DateTime `json:"acknowledgementDate"`
	HasDetailedMessage  bool      `json:"hasDetailedMesasage"`
}

type ScheduleDTO struct {
	ID                   int64 `json:"id"`
	IsActive             bool  `json:"isActive"`
	WorkerID             int64 `json:"workerID"`
	Sunday               bool  `json:"sunday"`
	Monday               bool  `json:"monday"`
	Tuesday              bool  `json:"tuesday"`
	Wednesday            bool  `json:"wednesday"`
	Thursday             bool  `json:"thursday"`
	Friday               bool  `json:"friday"`
	Saturday             bool  `json:"saturday"`
	TimeOfDay            Time  `json:"timeOfDay"`
	RecurTime            Time  `json:"recurTime"`
	RecurBetweenStartUTC Time  `json:"recurBetweenStartUTC"`
	RecurBetweenEndUTC   Time  `json:"recurBetweenEndUTC"`
	OneTime              bool  `json:"oneTime"`
}

type UserDTO struct {
	EmailAddress string `json:"emailAddress"`
}

type WorkerDTO struct {
	ID                  int64  `json:"id"`
	IsActive            bool   `json:"isActive"`
	WorkerName          string `json:"workerName"`
	DetailedDescription string `json:"detailedDescription"`
	EmailOnSuccess      string `json:"emailOnSuccess"`
	ParentWorkerID      *int64 `json:"parentWorkerID"`
	TimeoutMinutes      int64  `json:"timeoutMinutes"`
	DirectoryName       string `json:"directoryName"`
	Executable          string `json:"executable"`
	ArgumentValues      string `json:"argumentValues"`
}

package models

type Schedule struct {
	ID                   int64  `json:"id"`
	IsActive             bool   `json:"isActive"`
	WorkerID             int64  `json:"workerID"`
	Sunday               bool   `json:"sunday"`
	Monday               bool   `json:"monday"`
	Tuesday              bool   `json:"tuesday"`
	Wednesday            bool   `json:"wednesday"`
	Thursday             bool   `json:"thursday"`
	Friday               bool   `json:"friday"`
	Saturday             bool   `json:"saturday"`
	TimeOfDayUTC         string `json:"timeOfDayUTC"`
	RecurTime            string `json:"recurTime"`
	RecurBetweenStartUTC string `json:"recurBetweenStartUTC"`
	RecurBetweenEndUTC   string `json:"recurBetweenEndUTC"`
	OneTime              bool   `json:"oneTime"`
}

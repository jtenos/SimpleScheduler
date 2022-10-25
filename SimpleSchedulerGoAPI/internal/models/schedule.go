package models

import "time"

type Schedule struct {
	ID                   int64      `json:"id"`
	IsActive             bool       `json:"isActive"`
	WorkerID             int64      `json:"workerID"`
	Sunday               bool       `json:"sunday"`
	Monday               bool       `json:"monday"`
	Tuesday              bool       `json:"tuesday"`
	Wednesday            bool       `json:"wednesday"`
	Thursday             bool       `json:"thursday"`
	Friday               bool       `json:"friday"`
	Saturday             bool       `json:"saturday"`
	TimeOfDayUTC         *time.Time `json:"timeOfDayUTC"`
	RecurTime            *time.Time `json:"recurTime"`
	RecurBetweenStartUTC *time.Time `json:"recurBetweenStartUTC"`
	RecurBetweenEndUTC   *time.Time `json:"recurBetweenEndUTC"`
	OneTime              bool       `json:"oneTime"`
}

func NewSchedule(id int64, isActive bool, workerID int64, sunday bool, monday bool,
	tuesday bool, wednesday bool, thursday bool, friday bool, saturday bool,
	timeOfDayUTC *time.Time, recurTime *time.Time, recurBetweenStartUTC *time.Time,
	recurBetweenEndUTC *time.Time, oneTime bool,
) *Schedule {
	return &Schedule{id, isActive, workerID, sunday, monday,
		tuesday, wednesday, thursday, friday, saturday,
		timeOfDayUTC, recurTime, recurBetweenStartUTC,
		recurBetweenEndUTC, oneTime,
	}
}

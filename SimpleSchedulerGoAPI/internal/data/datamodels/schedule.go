package datamodels

import "time"

type Schedule struct {
	ID                   int64      `db:"ID"`
	IsActive             bool       `db:"IsActive"`
	WorkerID             int64      `db:"WorkerID"`
	Sunday               bool       `db:"Sunday"`
	Monday               bool       `db:"Monday"`
	Tuesday              bool       `db:"Tuesday"`
	Wednesday            bool       `db:"Wednesday"`
	Thursday             bool       `db:"Thursday"`
	Friday               bool       `db:"Friday"`
	Saturday             bool       `db:"Saturday"`
	TimeOfDayUTC         *time.Time `db:"TimeOfDayUTC"`
	RecurTime            *time.Time `db:"RecurTime"`
	RecurBetweenStartUTC *time.Time `db:"RecurBetweenStartUTC"`
	RecurBetweenEndUTC   *time.Time `db:"RecurBetweenEndUTC"`
	OneTime              bool       `db:"OneTime"`
}

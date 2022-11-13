package datamodels

import (
	"database/sql"
	"time"
)

// [app].[Schedules table]
type Schedule struct {
	ID        int64 `db:"ID"`
	IsActive  bool  `db:"IsActive"`
	WorkerID  int64 `db:"WorkerID"`
	Sunday    bool  `db:"Sunday"`
	Monday    bool  `db:"Monday"`
	Tuesday   bool  `db:"Tuesday"`
	Wednesday bool  `db:"Wednesday"`
	Thursday  bool  `db:"Thursday"`
	Friday    bool  `db:"Friday"`
	Saturday  bool  `db:"Saturday"`

	// TODO: Find a better way to serialize the time values:
	// https://stackoverflow.com/questions/23695479/how-to-format-timestamp-in-outgoing-json
	TimeOfDayUTC         *time.Time `db:"TimeOfDayUTC"`
	RecurTime            *time.Time `db:"RecurTime"`
	RecurBetweenStartUTC *time.Time `db:"RecurBetweenStartUTC"`
	RecurBetweenEndUTC   *time.Time `db:"RecurBetweenEndUTC"`
	OneTime              bool       `db:"OneTime"`
}

func (s *Schedule) Hydrate(rows *sql.Rows) error {
	return rows.Scan(&s.ID, &s.IsActive, &s.WorkerID, &s.Sunday, &s.Monday, &s.Tuesday,
		&s.Wednesday, &s.Thursday, &s.Friday, &s.Saturday, &s.TimeOfDayUTC, &s.RecurTime,
		&s.RecurBetweenStartUTC, &s.RecurBetweenEndUTC, &s.OneTime)
}

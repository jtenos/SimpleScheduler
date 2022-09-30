package models

import (
	"encoding/json"
	"time"
)

type CustomTime struct {
	Time time.Time
}

func (ct *CustomTime) UnmarshalJSON(b []byte) error {
	var s string
	err := json.Unmarshal(b, &s)
	if err != nil {
		return err
	}
	t, err := time.Parse("2006-01-02T15:04:05.9999999", s)
	if err != nil {
		return err
	}

	ct.Time = t
	return nil
}

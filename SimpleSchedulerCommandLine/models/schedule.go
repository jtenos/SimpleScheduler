package models

import (
	"fmt"
	"strconv"
	"strings"
)

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

func tern(b bool, first string, second string) string {
	if b {
		return first
	}
	return second
}

func getFormattedTime(ts string) string {
	if len(ts) == 0 {
		return ""
	}

	fields := strings.Split(ts, ":")
	hours, _ := strconv.Atoi(fields[0])
	minutes, _ := strconv.Atoi(fields[1])

	if hours == 1 && minutes == 0 {
		return "every hour"
	}
	if hours > 1 && minutes == 0 {
		return fmt.Sprintf("every %v hours", hours)
	}
	if hours > 0 && minutes > 0 {
		return fmt.Sprintf("every %s", ts)
	}
	if minutes == 1 {
		return "every minute"
	}
	if minutes > 1 {
		return fmt.Sprintf("every %v minutes", minutes)
	}
	return "unknown"
}

func (sch *Schedule) GetFormatted() string {
	var days string
	if sch.Sunday && sch.Monday && sch.Tuesday && sch.Wednesday && sch.Thursday && sch.Friday && sch.Saturday {
		days = "Every day"
	} else if !sch.Sunday && sch.Monday && sch.Tuesday && sch.Wednesday && sch.Thursday && sch.Friday && !sch.Saturday {
		days = "Weekdays"
	} else if sch.Sunday && !sch.Monday && !sch.Tuesday && !sch.Wednesday && !sch.Thursday && !sch.Friday && sch.Saturday {
		days = "Weekends"
	} else {
		days = fmt.Sprintf("%s %s %s %s %s %s %s",
			tern(sch.Sunday, "Su", "__"),
			tern(sch.Monday, "Mo", "__"),
			tern(sch.Tuesday, "Tu", "__"),
			tern(sch.Wednesday, "We", "__"),
			tern(sch.Thursday, "Th", "__"),
			tern(sch.Friday, "Fr", "__"),
			tern(sch.Saturday, "Sa", "__"),
		)
	}

	times := "Unknown"

	if len(sch.TimeOfDayUTC) > 0 {
		times = fmt.Sprintf("at %s", sch.TimeOfDayUTC)
	} else if len(sch.RecurTime) > 0 {
		times = getFormattedTime(sch.RecurTime)
	}

	if len(sch.RecurBetweenStartUTC) > 0 && len(sch.RecurBetweenEndUTC) > 0 {
		times += fmt.Sprintf(" between %s and %s", sch.RecurBetweenStartUTC, sch.RecurBetweenEndUTC)
	} else if len(sch.RecurBetweenStartUTC) > 0 {
		times += fmt.Sprintf(" starting at %s", sch.RecurBetweenStartUTC)
	} else if len(sch.RecurBetweenEndUTC) > 0 {
		times += fmt.Sprintf(" until %s", sch.RecurBetweenEndUTC)
	}

	return fmt.Sprintf("%s [%s]", days, times)
}

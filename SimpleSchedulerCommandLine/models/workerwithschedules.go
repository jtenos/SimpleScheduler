package models

type WorkerWithSchedules struct {
	Worker    Worker     `json:"worker"`
	Schedules []Schedule `json:"schedules"`
}

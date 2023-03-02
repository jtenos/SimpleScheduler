package models

type Worker struct {
	ID             int64       `json:"id"`
	IsActive       bool        `json:"isActive"`
	Name           string      `json:"name"`
	Description    string      `json:"description"`
	EmailOnSuccess string      `json:"emailOnSuccess"`
	ParentWorkerID *int64      `json:"parentWorkerID"`
	TimeoutMinutes int32       `json:"timeoutMinutes"`
	Directory      string      `json:"directory"`
	Executable     string      `json:"executable"`
	Args           string      `json:"args"`
	Schedules      []*Schedule `json:"schedules"`
}

func NewWorker(id int64, isActive bool, name string, description string,
	emailOnSuccess string, parentWorkerID *int64, timeoutMinutes int32,
	directory string, executable string, args string, schedules []*Schedule) *Worker {

	return &Worker{
		id,
		isActive,
		name,
		description,
		emailOnSuccess,
		parentWorkerID,
		timeoutMinutes,
		directory,
		executable,
		args,
		schedules,
	}
}

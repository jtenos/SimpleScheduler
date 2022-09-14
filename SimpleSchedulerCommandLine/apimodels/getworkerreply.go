package apimodels

import "github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/models"

type GetWorkerReply struct {
	Worker *models.WorkerWithSchedules `json:"worker"`
}

func NewGetWorkerReply() *GetWorkerReply {
	return &GetWorkerReply{}
}

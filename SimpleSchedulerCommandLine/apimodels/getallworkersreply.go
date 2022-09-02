package apimodels

import "github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/models"

type GetAllWorkersReply struct {
	Workers []models.WorkerWithSchedules `json:"workers"`
}

func NewGetAllWorkersReply() *GetAllWorkersReply {
	return &GetAllWorkersReply{}
}

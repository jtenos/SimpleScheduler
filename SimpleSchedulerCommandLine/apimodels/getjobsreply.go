package apimodels

import "github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/models"

type GetJobsReply struct {
	Jobs []models.JobWithWorkerID `json:"Jobs"`
}

func NewGetJobsReply() *GetJobsReply {
	return &GetJobsReply{}
}

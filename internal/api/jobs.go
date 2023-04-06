package api

import (
	"context"
	"encoding/json"
	"net/http"
	"time"

	"github.com/jtenos/simplescheduler/internal/api/errorhandling"
	"github.com/jtenos/simplescheduler/internal/data"
	"github.com/julienschmidt/httprouter"
)

type JobsHandler struct {
	ctx context.Context
}

func NewJobsHandler(ctx context.Context) *JobsHandler {
	return &JobsHandler{ctx}
}

type jobPostRequest struct {
	ScheduleID   int64     `json:"scheduleID"`
	QueueDateUTC time.Time `json:"queueDateUTC"`
}

func (h *JobsHandler) Post(w http.ResponseWriter, r *http.Request, ps httprouter.Params) {
	var req jobPostRequest
	decoder := json.NewDecoder(r.Body)
	err := decoder.Decode(&req)

	if err != nil {
		errorhandling.HandleError(w, r, err, "JobsHandler.Post", http.StatusInternalServerError)
		return
	}
	jobRepo := data.NewJobRepo(h.ctx)
	jobID, err := jobRepo.CreateJob(req.ScheduleID, req.QueueDateUTC)
	if err != nil {
		errorhandling.HandleError(w, r, err, "JobsHandler.Post", http.StatusInternalServerError)
		return
	}

	json.NewEncoder(w).Encode(struct{ jobID int64 }{jobID: jobID})
}

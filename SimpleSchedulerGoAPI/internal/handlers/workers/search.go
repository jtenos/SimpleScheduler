package workers

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/errors"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/models"
)

type SearchHandler struct {
	ctx     context.Context
	connStr string
}

func NewSearchHandler(ctx context.Context, connStr string) *SearchHandler {
	return &SearchHandler{ctx, connStr}
}

type searchReply struct {
	Workers []*models.Worker `json:"workers"`
}

func (h *SearchHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	q := r.URL.Query()
	nameFilter := q.Get("name")
	directoryFilter := q.Get("directory")
	executableFilter := q.Get("executable")
	statusFilter := q.Get("status")

	workerRepo := data.NewWorkerRepo(h.connStr)
	workers, err := workerRepo.Search(h.ctx, nameFilter, directoryFilter, executableFilter, statusFilter)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error(), false)
		return
	}
	json.NewEncoder(w).Encode(searchReply{Workers: workers})
}

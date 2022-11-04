package workers

import (
	"context"
	"encoding/json"
	"net/http"
	"strconv"

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
	idFilter := q.Get("id")
	parentIdFilter := q.Get("parent")
	nameFilter := q.Get("name")
	directoryFilter := q.Get("directory")
	executableFilter := q.Get("executable")
	statusFilter := q.Get("status")

	var idsFilter []int64
	var parentWorkerIDFilter *int64

	if len(idFilter) > 0 {
		id, err := strconv.ParseInt(idFilter, 10, 64)
		if err != nil {
			errors.HandleError(w, r, http.StatusBadRequest, "invalid id parameter", true)
			return
		}
		idsFilter = []int64{id}
	}
	if len(parentIdFilter) > 0 {
		pid, err := strconv.ParseInt(parentIdFilter, 10, 64)
		if err != nil {
			errors.HandleError(w, r, http.StatusBadRequest, "invalid parent parameter", true)
			return
		}
		parentWorkerIDFilter = &pid
	}

	workerRepo := data.NewWorkerRepo(h.connStr)
	workers, err := workerRepo.Search(h.ctx, idsFilter, parentWorkerIDFilter,
		nameFilter, directoryFilter, executableFilter, statusFilter)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error(), false)
		return
	}
	json.NewEncoder(w).Encode(searchReply{Workers: workers})
}

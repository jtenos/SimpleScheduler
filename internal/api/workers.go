package api

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"strconv"

	"github.com/julienschmidt/httprouter"
	"jtenos.com/simplescheduler/internal/api/errorhandling"
)

type WorkersHandler struct {
	ctx     context.Context
	connStr string
}

func NewWorkersHandler(ctx context.Context, connStr string) *WorkersHandler {
	return &WorkersHandler{ctx, connStr}
}

func (h *WorkersHandler) Get(w http.ResponseWriter, r *http.Request, _ httprouter.Params) {
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
			errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("invalid id parameter"), "SearchHandler.ServeHTTP")
			return
		}
		idsFilter = []int64{id}
	}
	if len(parentIdFilter) > 0 {
		pid, err := strconv.ParseInt(parentIdFilter, 10, 64)
		if err != nil {
			errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("invalid parent parameter"), "SearchHandler.ServeHTTP")
			return
		}
		parentWorkerIDFilter = &pid
	}

	workers := fmt.Sprintf("%s%s%s%s", nameFilter, directoryFilter, executableFilter, statusFilter)
	fmt.Println(idsFilter)
	fmt.Println(parentWorkerIDFilter)

	// workerRepo := data.NewWorkerRepo(h.connStr)
	// workers, err := workerRepo.Search(h.ctx, idsFilter, parentWorkerIDFilter,
	// 	nameFilter, directoryFilter, executableFilter, statusFilter)
	// if err != nil {
	// 	errorhandling.HandleError(w, r, errorhandling.NewInternalServerError(err.Error()), "SearchHandler.ServeHTTP")
	// 	return
	// }
	json.NewEncoder(w).Encode(workers)
}

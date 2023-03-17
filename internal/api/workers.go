package api

import (
	"context"
	"encoding/json"
	"errors"
	"net/http"
	"strconv"

	"github.com/jtenos/simplescheduler/internal/api/errorhandling"
	"github.com/jtenos/simplescheduler/internal/data"
	"github.com/julienschmidt/httprouter"
)

type WorkersHandler struct {
	ctx context.Context
}

type workerModel struct {
	ID                  int64  `json:"id"`
	IsActive            bool   `json:"isActive"`
	WorkerName          string `json:"workerName"`
	DetailedDescription string `json:"detailedDescription"`
	EmailOnSuccess      string `json:"emailOnSuccess"`
	ParentWorkerID      *int64 `json:"parentWorkerID"`
	TimeoutMinutes      int32  `json:"timeoutMinutes"`
	DirectoryName       string `json:"directoryName"`
	Executable          string `json:"executable"`
	ArgumentValues      string `json:"argumentValues"`
}

func NewWorkersHandler(ctx context.Context) *WorkersHandler {
	return &WorkersHandler{ctx}
}

func (h *WorkersHandler) Get(w http.ResponseWriter, r *http.Request, ps httprouter.Params) {

	idFilter := ps.ByName("id")
	if len(idFilter) > 0 {
		id, err := strconv.ParseInt(idFilter, 10, 64)
		if err != nil {
			errorhandling.HandleError(w, r, errors.New("invalid id parameter"), "WorkersHandler.Get", http.StatusBadRequest)
			return
		}
		h.getByID(w, r, id)
		return
	}

	h.search(w, r)
}

func (h *WorkersHandler) search(w http.ResponseWriter, r *http.Request) {
	// q := r.URL.Query()

	// parentIdFilter := q.Get("parent")
	// nameFilter := q.Get("name")
	// directoryFilter := q.Get("directory")
	// executableFilter := q.Get("executable")
	// statusFilter := q.Get("status")

	// var parentWorkerIDFilter *int64

	// if len(parentIdFilter) > 0 {
	// 	pid, err := strconv.ParseInt(parentIdFilter, 10, 64)
	// 	if err != nil {
	// 		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("invalid parent parameter"), "WorkersHandler.search")
	// 		return
	// 	}
	// 	parentWorkerIDFilter = &pid
	// }

	// // workerRepo := data.NewWorkerRepo(h.connStr)
	// // workers, err := workerRepo.Search(h.ctx, idsFilter, parentWorkerIDFilter,
	// // 	nameFilter, directoryFilter, executableFilter, statusFilter)
	// // if err != nil {
	// // 	errorhandling.HandleError(w, r, errorhandling.NewInternalServerError(err.Error()), "SearchHandler.ServeHTTP")
	// // 	return
	// // }
	// json.NewEncoder(w).Encode(workers)
}

func (h *WorkersHandler) getByID(w http.ResponseWriter, r *http.Request, id int64) {

	type getByIdReply struct {
		workerModel
	}

	workerRepo := data.NewWorkerRepo(h.ctx)
	dmWorker, err := workerRepo.GetByID(id)
	if err != nil {
		errorhandling.HandleError(w, r, err, "WorkersHandler.getByID", http.StatusInternalServerError)
		return
	}

	reply := getByIdReply{
		workerModel: workerModel{
			ID:                  dmWorker.ID,
			IsActive:            dmWorker.IsActive,
			WorkerName:          dmWorker.WorkerName,
			DetailedDescription: dmWorker.DetailedDescription,
			EmailOnSuccess:      dmWorker.EmailOnSuccess,
			ParentWorkerID:      dmWorker.ParentWorkerID,
			TimeoutMinutes:      dmWorker.TimeoutMinutes,
			DirectoryName:       dmWorker.DirectoryName,
			Executable:          dmWorker.Executable,
			ArgumentValues:      dmWorker.ArgumentValues,
		},
	}

	json.NewEncoder(w).Encode(reply)
}

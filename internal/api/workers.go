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
		h.getByID(w, r, idFilter)
		return
	}

	h.search(w, r, ps)
}

func (h *WorkersHandler) Post(w http.ResponseWriter, r *http.Request, ps httprouter.Params) {
	panic("Not Implemented")
}

func (h *WorkersHandler) Put(w http.ResponseWriter, r *http.Request, ps httprouter.Params) {
	panic("Not Implemented")
}

func (h *WorkersHandler) Delete(w http.ResponseWriter, r *http.Request, ps httprouter.Params) {
	panic("Not Implemented")
}

func (h *WorkersHandler) search(w http.ResponseWriter, r *http.Request, ps httprouter.Params) {

	nameFilter := ps.ByName("name")
	descFilter := ps.ByName("description")
	dirFilter := ps.ByName("directory")
	exeFilter := ps.ByName("executable")
	activeFilter := ps.ByName("active")
	parentFilter := ps.ByName("parent")

	workerRepo := data.NewWorkerRepo(h.ctx)

	workers, err := workerRepo.Search(h.ctx, nameFilter, descFilter, dirFilter,
		exeFilter, activeFilter, parentFilter)
	if err != nil {
		errorhandling.HandleError(w, r, err, "SearchHandler.ServeHTTP", http.StatusInternalServerError)
		return
	}
	json.NewEncoder(w).Encode(workers)
}

func (h *WorkersHandler) getByID(w http.ResponseWriter, r *http.Request, idFilter string) {

	id, err := strconv.ParseInt(idFilter, 10, 64)
	if err != nil {
		errorhandling.HandleError(w, r, errors.New("invalid id parameter"), "WorkersHandler.Get", http.StatusBadRequest)
		return
	}

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

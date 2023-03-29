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
	// var worker models.Worker
	// 	decoder := json.NewDecoder(r.Body)
	// 	decoder.Decode(&worker)

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

	workerRepo := data.NewWorkerRepo(h.ctx)
	dmWorker, err := workerRepo.GetByID(id)
	if err != nil {
		errorhandling.HandleError(w, r, err, "WorkersHandler.getByID", http.StatusInternalServerError)
		return
	}

	reply := workerModel{
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
	}

	json.NewEncoder(w).Encode(reply)
}

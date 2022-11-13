package workers

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/models"
)

type CreateHandler struct {
	ctx        context.Context
	connStr    string
	workerPath string
}

func NewCreateHandler(ctx context.Context, connStr string, workerPath string) *CreateHandler {
	return &CreateHandler{ctx, connStr, workerPath}
}

func (h *CreateHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	var worker models.Worker
	decoder := json.NewDecoder(r.Body)
	decoder.Decode(&worker)

	repo := data.NewWorkerRepo(h.connStr)
	_, err := repo.Create(h.ctx, worker.Name, worker.Description, worker.EmailOnSuccess, worker.ParentWorkerID,
		worker.TimeoutMinutes, worker.Directory, worker.Executable, worker.Args, h.workerPath,
	)
	if err != nil {
		errorhandling.HandleError(w, r, err, "CreateHandler.ServeHTTP")
	}
}

/*
func (h *SearchHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	workerRepo := data.NewWorkerRepo(h.connStr)
	workers, err := workerRepo.Search(h.ctx, idsFilter, parentWorkerIDFilter,
		nameFilter, directoryFilter, executableFilter, statusFilter)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error(), false)
		return
	}
	json.NewEncoder(w).Encode(searchReply{Workers: workers})
}

*/

/*
namespace SimpleSchedulerApiModels.Request.Workers;

public record class CreateWorkerRequest(
    string WorkerName,
    string DetailedDescription,
    string EmailOnSuccess,
    long? ParentWorkerID,
    int TimeoutMinutes,
    string DirectoryName,
    string Executable,
    string ArgumentValues
);

*/

// package workers

// import (
// 	"context"
// 	"encoding/json"
// 	"fmt"
// 	"net/http"

// 	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
// 	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
// 	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/models"
// )

// type CreateHandler struct {
// 	ctx        context.Context
// 	connStr    string
// 	workerPath string
// }

// func NewCreateHandler(ctx context.Context, connStr string, workerPath string) *CreateHandler {
// 	return &CreateHandler{ctx, connStr, workerPath}
// }

// func (h *CreateHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

// 	var worker models.Worker
// 	decoder := json.NewDecoder(r.Body)
// 	decoder.Decode(&worker)

// 	repo := data.NewWorkerRepo(h.connStr)
// 	err := repo.Create(h.ctx, worker.Name, worker.Description, worker.EmailOnSuccess, worker.ParentWorkerID,
// 		worker.TimeoutMinutes, worker.Directory, worker.Executable, worker.Args, h.workerPath,
// 	)
// 	if err != nil {
// 		errorhandling.HandleError(w, r, err, "CreateHandler.ServeHTTP")
// 		return
// 	}
// 	fmt.Fprint(w, "{}")
// }

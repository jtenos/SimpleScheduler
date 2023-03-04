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

// type UpdateHandler struct {
// 	ctx        context.Context
// 	connStr    string
// 	workerPath string
// }

// func NewUpdateHandler(ctx context.Context, connStr string, workerPath string) *UpdateHandler {
// 	return &UpdateHandler{ctx, connStr, workerPath}
// }

// func (h *UpdateHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

// 	var worker models.Worker
// 	decoder := json.NewDecoder(r.Body)
// 	decoder.Decode(&worker)

// 	repo := data.NewWorkerRepo(h.connStr)
// 	err := repo.Update(h.ctx, worker.ID, worker.Name, worker.Description,
// 		worker.EmailOnSuccess, worker.ParentWorkerID, worker.TimeoutMinutes,
// 		worker.Directory, worker.Executable, worker.Args, h.workerPath,
// 	)
// 	if err != nil {
// 		errorhandling.HandleError(w, r, err, "UpdateHandler.ServeHTTP")
// 		return
// 	}
// 	fmt.Fprint(w, "{}")
// }

// package workers

// import (
// 	"context"
// 	"encoding/json"
// 	"fmt"
// 	"net/http"

// 	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
// 	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
// )

// type RunHandler struct {
// 	ctx     context.Context
// 	connStr string
// }

// type runRequest struct {
// 	ID int64 `json:"ID"`
// }

// func NewRunHandler(ctx context.Context, connStr string) *RunHandler {
// 	return &RunHandler{ctx, connStr}
// }

// func (h *RunHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

// 	var runReq runRequest
// 	decoder := json.NewDecoder(r.Body)
// 	err := decoder.Decode(&runReq)
// 	if err != nil {
// 		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("id is required"), "RunHandler.ServeHTTP")
// 		return
// 	}

// 	repo := data.NewWorkerRepo(h.connStr)
// 	err = repo.RunNow(h.ctx, runReq.ID)
// 	if err != nil {
// 		errorhandling.HandleError(w, r, err, "RunHandler.ServeHTTP")
// 		return
// 	}
// 	fmt.Fprint(w, "{}")
// }

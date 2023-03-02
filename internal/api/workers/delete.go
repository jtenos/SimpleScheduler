// package workers

// import (
// 	"context"
// 	"encoding/json"
// 	"fmt"
// 	"net/http"

// 	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
// 	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
// )

// type DeleteHandler struct {
// 	ctx     context.Context
// 	connStr string
// }

// type deleteRequest struct {
// 	ID int64 `json:"ID"`
// }

// func NewDeleteHandler(ctx context.Context, connStr string) *DeleteHandler {
// 	return &DeleteHandler{ctx, connStr}
// }

// func (h *DeleteHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

// 	var delReq deleteRequest
// 	decoder := json.NewDecoder(r.Body)
// 	err := decoder.Decode(&delReq)
// 	if err != nil {
// 		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("id is required"), "DeleteHandler.ServeHTTP")
// 		return
// 	}

// 	repo := data.NewWorkerRepo(h.connStr)
// 	err = repo.Delete(h.ctx, delReq.ID)
// 	if err != nil {
// 		errorhandling.HandleError(w, r, err, "DeleteHandler.ServeHTTP")
// 		return
// 	}
// 	fmt.Fprint(w, "{}")
// }

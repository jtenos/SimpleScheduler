package workers

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
)

type ReactivateHandler struct {
	ctx     context.Context
	connStr string
}

type reactivateRequest struct {
	ID int64 `json:"ID"`
}

func NewReactivateHandler(ctx context.Context, connStr string) *ReactivateHandler {
	return &ReactivateHandler{ctx, connStr}
}

func (h *ReactivateHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	var reactReq reactivateRequest
	decoder := json.NewDecoder(r.Body)
	err := decoder.Decode(&reactReq)
	if err != nil {
		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("id is required"), "ReactivateHandler.ServeHTTP")
		return
	}

	repo := data.NewWorkerRepo(h.connStr)
	err = repo.Reactivate(h.ctx, reactReq.ID)
	if err != nil {
		errorhandling.HandleError(w, r, err, "ReactivateHandler.ServeHTTP")
		return
	}
	fmt.Fprint(w, "{}")
}
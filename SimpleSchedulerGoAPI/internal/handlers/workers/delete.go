package workers

import (
	"context"
	"fmt"
	"net/http"
	"strconv"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
)

type DeleteHandler struct {
	ctx     context.Context
	connStr string
}

func NewDeleteHandler(ctx context.Context, connStr string) *DeleteHandler {
	return &DeleteHandler{ctx, connStr}
}

func (h *DeleteHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	id, err := strconv.ParseInt(r.URL.Query().Get("id"), 10, 64)
	if err != nil {
		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("id is required"), "DeleteHandler.ServeHTTP")
	}

	repo := data.NewWorkerRepo(h.connStr)
	err = repo.Delete(h.ctx, id)
	if err != nil {
		errorhandling.HandleError(w, r, err, "DeleteHandler.ServeHTTP")
	}
	fmt.Fprint(w, "{}")
}

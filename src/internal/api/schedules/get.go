// q := r.URL.Query()
// idFilter := q.Get("id")
// parentIdFilter := q.Get("parent")
// nameFilter := q.Get("name")
// directoryFilter := q.Get("directory")
// executableFilter := q.Get("executable")
// statusFilter := q.Get("status")

package schedules

import (
	"context"
	"encoding/json"
	"net/http"
	"strconv"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
)

type GetHandler struct {
	ctx     context.Context
	connStr string
}

func NewGetHandler(ctx context.Context, connStr string) *GetHandler {
	return &GetHandler{ctx, connStr}
}

func (h *GetHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	id, err := strconv.ParseInt(r.URL.Query().Get("id"), 10, 64)
	if err != nil {
		errorhandling.HandleError(w, r, err, "GetHandler.ServeHTTP")
	}

	repo := data.NewScheduleRepo(h.connStr)
	sched, err := repo.Get(h.ctx, id)
	if err != nil {
		errorhandling.HandleError(w, r, err, "GetHandler.ServeHTTP")
		return
	}

	json.NewEncoder(w).Encode(sched)
}

package schedules

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/models"
)

type CreateHandler struct {
	ctx     context.Context
	connStr string
}

func NewCreateHandler(ctx context.Context, connStr string) *CreateHandler {
	return &CreateHandler{ctx, connStr}
}

func (h *CreateHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	var schedule models.Schedule
	decoder := json.NewDecoder(r.Body)
	decoder.Decode(&schedule)

	repo := data.NewScheduleRepo(h.connStr)
	err := repo.Create(h.ctx, schedule.WorkerID, schedule.Sunday, schedule.Monday, schedule.Tuesday,
		schedule.Wednesday, schedule.Thursday, schedule.Friday, schedule.Saturday,
		schedule.TimeOfDayUTC, schedule.RecurTime, schedule.RecurBetweenStartUTC,
		schedule.RecurBetweenEndUTC,
	)
	if err != nil {
		errorhandling.HandleError(w, r, err, "CreateHandler.ServeHTTP")
		return
	}
	fmt.Fprint(w, "{}")
}

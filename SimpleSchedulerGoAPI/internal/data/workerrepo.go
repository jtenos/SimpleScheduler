package data

import (
	"context"
	"database/sql"
	"encoding/json"
	"errors"
	"log"
	"os"
	"path"
	"strings"

	"github.com/jmoiron/sqlx"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/datamodels"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/models"
)

type WorkerRepo struct {
	connStr string
}

func NewWorkerRepo(connStr string) WorkerRepo {
	return WorkerRepo{connStr}
}

func isValidExec(dir string, exe string, workerPath string) bool {
	if strings.Contains(dir, "/") || strings.Contains(dir, "\\") || strings.Contains(exe, "/") || strings.Contains(exe, "\\") {
		return false
	}

	fullPath := path.Join(workerPath, dir, exe)
	_, err := os.Lstat(fullPath)

	return err == nil
}

func (r WorkerRepo) Create(ctx context.Context, name string, description string, emailOnSuccess string, parentWorkerID *int64,
	timeoutMinutes int32, directory string, executable string, args string, workerPath string) (worker *models.Worker, err error) {

	if !isValidExec(directory, executable, workerPath) {
		err = errors.New("invalid executable")
		return
	}

	db, err := sqlx.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	type insertResult struct {
		ID                int64 `db:"ID"`
		Success           bool  `db:"Success"`
		NameAlreadyExists bool  `db:"NameAlreadyExists"`
		CircularReference bool  `db:"CircularReference"`
	}

	var res insertResult

	err = db.GetContext(ctx, &res, "[app].[Workers_Insert]",
		sql.Named("WorkerName", name),
		sql.Named("DetailedDescription", description),
		sql.Named("EmailOnSuccess", emailOnSuccess),
		sql.Named("ParentWorkerID", parentWorkerID),
		sql.Named("TimeoutMinutes", timeoutMinutes),
		sql.Named("DirectoryName", directory),
		sql.Named("Executable", executable),
		sql.Named("ArgumentValues", args),
	)
	if err != nil {
		return
	}
	if res.CircularReference {
		err = errorhandling.NewBadRequestError("circular reference")
		return
	}
	if res.NameAlreadyExists {
		err = errorhandling.NewBadRequestError("name already exists")
		return
	}
	if !res.Success {
		err = errors.New("unknown error")
		return
	}
	workers, err := r.Search(ctx, []int64{res.ID}, nil, "", "", "", "")
	worker = workers[0]
	log.Println(worker)
	return
}

func (r WorkerRepo) Search(ctx context.Context, idsFilter []int64, parentWorkerIDFilter *int64,
	nameFilter string, directoryFilter string, executableFilter string, statusFilter string) (workers []*models.Worker, err error) {

	db, err := sqlx.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	activeOnly := strings.EqualFold(statusFilter, "active")
	inactiveOnly := strings.EqualFold(statusFilter, "inactive")

	idsJson, _ := json.Marshal(idsFilter)

	var workerDMs []datamodels.Worker
	err = db.SelectContext(ctx, &workerDMs, "[app].[Workers_Select]",
		sql.Named("IDs", string(idsJson)),
		sql.Named("ParentWorkerID", parentWorkerIDFilter),
		sql.Named("WorkerName", nameFilter),
		sql.Named("DirectoryName", directoryFilter),
		sql.Named("Executable", executableFilter),
		sql.Named("ActiveOnly", activeOnly),
		sql.Named("InactiveOnly", inactiveOnly),
	)
	if err != nil {
		return
	}

	log.Printf("Num workerDMs: %d", len(workerDMs))

	workerIDs := make([]int64, len(workerDMs))
	for i := range workerDMs {
		workerIDs[i] = workerDMs[i].ID
	}

	workerIDsJson, _ := json.Marshal(workerIDs)

	var scheduleDMs []datamodels.Schedule
	err = db.SelectContext(ctx, &scheduleDMs, "[app].[Schedules_Select]",
		sql.Named("WorkerIDs", string(workerIDsJson)),
	)
	if err != nil {
		return
	}

	log.Printf("Num scheduleDMs: %d", len(scheduleDMs))

	// WorkerID, Worker
	workerMap := map[int64]*models.Worker{}

	for i := range workerDMs {
		workerMap[workerDMs[i].ID] = models.NewWorker(
			workerDMs[i].ID,
			workerDMs[i].IsActive,
			workerDMs[i].WorkerName,
			workerDMs[i].DetailedDescription,
			workerDMs[i].EmailOnSuccess,
			workerDMs[i].ParentWorkerID,
			workerDMs[i].TimeoutMinutes,
			workerDMs[i].DirectoryName,
			workerDMs[i].Executable,
			workerDMs[i].ArgumentValues,
			[]*models.Schedule{},
		)
	}

	for i := range scheduleDMs {
		w := workerMap[scheduleDMs[i].WorkerID]
		w.Schedules = append(w.Schedules, models.NewSchedule(
			scheduleDMs[i].ID,
			scheduleDMs[i].IsActive,
			scheduleDMs[i].WorkerID,
			scheduleDMs[i].Sunday,
			scheduleDMs[i].Monday,
			scheduleDMs[i].Tuesday,
			scheduleDMs[i].Wednesday,
			scheduleDMs[i].Thursday,
			scheduleDMs[i].Friday,
			scheduleDMs[i].Saturday,
			scheduleDMs[i].TimeOfDayUTC,
			scheduleDMs[i].RecurTime,
			scheduleDMs[i].RecurBetweenStartUTC,
			scheduleDMs[i].RecurBetweenEndUTC,
			scheduleDMs[i].OneTime,
		))
	}

	for wid := range workerMap {
		workers = append(workers, workerMap[wid])
	}

	return
}

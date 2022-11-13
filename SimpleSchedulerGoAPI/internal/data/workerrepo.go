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

	db, err := sql.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	var id int64
	var success bool
	var nameAlreadyExists bool
	var circularReference bool

	row := db.QueryRowContext(ctx, "[app].[Workers_Insert]",
		sql.Named("WorkerName", name),
		sql.Named("DetailedDescription", description),
		sql.Named("EmailOnSuccess", emailOnSuccess),
		sql.Named("ParentWorkerID", parentWorkerID),
		sql.Named("TimeoutMinutes", timeoutMinutes),
		sql.Named("DirectoryName", directory),
		sql.Named("Executable", executable),
		sql.Named("ArgumentValues", args),
	)
	if err = row.Scan(&id, &success, &nameAlreadyExists, &circularReference); err != nil {
		return
	}
	if circularReference {
		err = errorhandling.NewBadRequestError("circular reference")
		return
	}
	if nameAlreadyExists {
		err = errorhandling.NewBadRequestError("name already exists")
		return
	}
	if !success {
		err = errors.New("unknown error")
		return
	}
	workers, err := r.Search(ctx, []int64{id}, nil, "", "", "", "")
	worker = workers[0]
	log.Println(worker)
	return
}

func (r WorkerRepo) Search(ctx context.Context, idsFilter []int64, parentWorkerIDFilter *int64,
	nameFilter string, directoryFilter string, executableFilter string, statusFilter string) (workers []*models.Worker, err error) {

	db, err := sql.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	activeOnly := strings.EqualFold(statusFilter, "active")
	inactiveOnly := strings.EqualFold(statusFilter, "inactive")

	var idsJson []byte
	if idsFilter != nil {
		idsJson, _ = json.Marshal(idsFilter)
	}

	var workerDMs []datamodels.Worker
	rows, err := db.QueryContext(ctx, "[app].[Workers_Select]",
		sql.Named("IDs", sql.NullString{String: string(idsJson), Valid: idsJson != nil}),
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
	defer rows.Close()
	for rows.Next() {
		var w datamodels.Worker
		err = w.Hydrate(rows)
		if err != nil {
			return
		}
		workerDMs = append(workerDMs, w)
	}

	log.Printf("Num workerDMs: %d", len(workerDMs))

	workerIDs := make([]int64, len(workerDMs))
	for i := range workerDMs {
		workerIDs[i] = workerDMs[i].ID
	}

	workerIDsJson, _ := json.Marshal(workerIDs)

	var scheduleDMs []datamodels.Schedule
	rows, err = db.QueryContext(ctx, "[app].[Schedules_Select]",
		sql.Named("WorkerIDs", string(workerIDsJson)),
	)
	if err != nil {
		return
	}
	for rows.Next() {
		var s datamodels.Schedule
		err = s.Hydrate(rows)
		if err != nil {
			return
		}
		scheduleDMs = append(scheduleDMs, s)
	}

	log.Printf("Num scheduleDMs: %d", len(scheduleDMs))

	for i := range scheduleDMs {
		log.Printf("%#v", scheduleDMs[i])
	}

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
		log.Printf("Looking up worker %d for schedule %d", scheduleDMs[i].WorkerID, scheduleDMs[i].ID)
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

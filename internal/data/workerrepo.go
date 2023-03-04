package data

import (
	"context"
	"database/sql"
	"errors"

	"jtenos.com/simplescheduler/internal/datamodels"
)

type WorkerRepo struct {
	db *DB
}

func NewWorkerRepo(ctx context.Context) *WorkerRepo {
	return &WorkerRepo{newDB(ctx)}
}

func (r *WorkerRepo) GetByID(id int64) (*datamodels.Worker, error) {
	err := r.db.open()
	if err != nil {
		return nil, err
	}
	defer r.db.close()

	rows, err := r.db.db.QueryContext(r.db.ctx, "SELECT * FROM [workers] WHERE [worker_id] = @worker_id;",
		sql.Named("worker_id", id),
	)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	if !rows.Next() {
		return nil, errors.New("worker not found")
	}
	var w datamodels.Worker
	err = w.Hydrate(rows)
	if err != nil {
		return nil, err
	}
	return &w, nil
}

// func isValidExec(dir string, exe string, workerPath string) bool {
// 	if strings.Contains(dir, "/") || strings.Contains(dir, "\\") || strings.Contains(exe, "/") || strings.Contains(exe, "\\") {
// 		return false
// 	}

// 	fullPath := path.Join(workerPath, dir, exe)
// 	_, err := os.Lstat(fullPath)

// 	return err == nil
// }

// func (r WorkerRepo) Create(ctx context.Context, name string, description string, emailOnSuccess string, parentWorkerID *int64,
// 	timeoutMinutes int32, directory string, executable string, args string, workerPath string) (err error) {

// 	if !isValidExec(directory, executable, workerPath) {
// 		err = errors.New("invalid executable")
// 		return
// 	}

// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	var id int64
// 	var success bool
// 	var nameAlreadyExists bool
// 	var circularReference bool

// 	row := db.QueryRowContext(ctx, "[app].[Workers_Insert]",
// 		sql.Named("WorkerName", name),
// 		sql.Named("DetailedDescription", description),
// 		sql.Named("EmailOnSuccess", emailOnSuccess),
// 		sql.Named("ParentWorkerID", parentWorkerID),
// 		sql.Named("TimeoutMinutes", timeoutMinutes),
// 		sql.Named("DirectoryName", directory),
// 		sql.Named("Executable", executable),
// 		sql.Named("ArgumentValues", args),
// 	)
// 	if err = row.Scan(&id, &success, &nameAlreadyExists, &circularReference); err != nil {
// 		return
// 	}
// 	if circularReference {
// 		err = errorhandling.NewBadRequestError("circular reference")
// 		return
// 	}
// 	if nameAlreadyExists {
// 		err = errorhandling.NewBadRequestError("name already exists")
// 		return
// 	}
// 	if !success {
// 		err = errors.New("unknown error")
// 		return
// 	}
// 	return
// }

// func (r WorkerRepo) Update(ctx context.Context, id int64, name string, description string,
// 	emailOnSuccess string, parentWorkerID *int64, timeoutMinutes int32, directory string,
// 	executable string, args string, workerPath string) (err error) {

// 	if !isValidExec(directory, executable, workerPath) {
// 		err = errors.New("invalid executable")
// 		return
// 	}

// 	if id == *parentWorkerID {
// 		err = errors.New("worker cannot be its own parent")
// 		return
// 	}

// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	var success bool
// 	var nameAlreadyExists bool
// 	var circularReference bool

// 	row := db.QueryRowContext(ctx, "[app].[Workers_Update]",
// 		sql.Named("ID", id),
// 		sql.Named("WorkerName", name),
// 		sql.Named("DetailedDescription", description),
// 		sql.Named("EmailOnSuccess", emailOnSuccess),
// 		sql.Named("ParentWorkerID", parentWorkerID),
// 		sql.Named("TimeoutMinutes", timeoutMinutes),
// 		sql.Named("DirectoryName", directory),
// 		sql.Named("Executable", executable),
// 		sql.Named("ArgumentValues", args),
// 	)
// 	if err = row.Scan(&success, &nameAlreadyExists, &circularReference); err != nil {
// 		return
// 	}
// 	if circularReference {
// 		err = errorhandling.NewBadRequestError("circular reference")
// 		return
// 	}
// 	if nameAlreadyExists {
// 		err = errorhandling.NewBadRequestError("name already exists")
// 		return
// 	}
// 	if !success {
// 		err = errors.New("unknown error")
// 		return
// 	}
// 	return
// }

// func (r WorkerRepo) Delete(ctx context.Context, id int64) (err error) {
// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	_, err = db.ExecContext(ctx, "[app].[Workers_Deactivate]",
// 		sql.Named("ID", id),
// 	)
// 	return
// }

// func (r WorkerRepo) Reactivate(ctx context.Context, id int64) (err error) {
// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	_, err = db.ExecContext(ctx, "[app].[Workers_Reactivate]",
// 		sql.Named("ID", id),
// 	)
// 	return
// }

// func (r WorkerRepo) RunNow(ctx context.Context, id int64) (err error) {
// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	_, err = db.ExecContext(ctx, "[app].[Jobs_RunNow]",
// 		sql.Named("ID", id),
// 	)
// 	return
// }

// func (r WorkerRepo) Search(ctx context.Context, idsFilter []int64, parentWorkerIDFilter *int64,
// 	nameFilter string, directoryFilter string, executableFilter string, statusFilter string) (workers []*models.Worker, err error) {

// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	activeOnly := strings.EqualFold(statusFilter, "active")
// 	inactiveOnly := strings.EqualFold(statusFilter, "inactive")

// 	var idsJson []byte
// 	if idsFilter != nil {
// 		idsJson, _ = json.Marshal(idsFilter)
// 	}

// 	var workerDMs []datamodels.Worker
// 	rows, err := db.QueryContext(ctx, "[app].[Workers_Select]",
// 		sql.Named("IDs", sql.NullString{String: string(idsJson), Valid: idsJson != nil}),
// 		sql.Named("ParentWorkerID", parentWorkerIDFilter),
// 		sql.Named("WorkerName", nameFilter),
// 		sql.Named("DirectoryName", directoryFilter),
// 		sql.Named("Executable", executableFilter),
// 		sql.Named("ActiveOnly", activeOnly),
// 		sql.Named("InactiveOnly", inactiveOnly),
// 	)
// 	if err != nil {
// 		return
// 	}
// 	defer rows.Close()
// 	for rows.Next() {
// 		var w datamodels.Worker
// 		err = w.Hydrate(rows)
// 		if err != nil {
// 			return
// 		}
// 		workerDMs = append(workerDMs, w)
// 	}

// 	log.Printf("Num workerDMs: %d", len(workerDMs))

// 	workerIDs := make([]int64, len(workerDMs))
// 	for i := range workerDMs {
// 		workerIDs[i] = workerDMs[i].ID
// 	}

// 	workerIDsJson, _ := json.Marshal(workerIDs)

// 	var scheduleDMs []datamodels.Schedule
// 	rows, err = db.QueryContext(ctx, "[app].[Schedules_Select]",
// 		sql.Named("WorkerIDs", string(workerIDsJson)),
// 	)
// 	if err != nil {
// 		return
// 	}
// 	for rows.Next() {
// 		var s datamodels.Schedule
// 		err = s.Hydrate(rows)
// 		if err != nil {
// 			return
// 		}
// 		scheduleDMs = append(scheduleDMs, s)
// 	}

// 	log.Printf("Num scheduleDMs: %d", len(scheduleDMs))

// 	for i := range scheduleDMs {
// 		log.Printf("%#v", scheduleDMs[i])
// 	}

// 	// WorkerID, Worker
// 	workerMap := map[int64]*models.Worker{}

// 	for i := range workerDMs {
// 		workerMap[workerDMs[i].ID] = models.NewWorker(
// 			workerDMs[i].ID,
// 			workerDMs[i].IsActive,
// 			workerDMs[i].WorkerName,
// 			workerDMs[i].DetailedDescription,
// 			workerDMs[i].EmailOnSuccess,
// 			workerDMs[i].ParentWorkerID,
// 			workerDMs[i].TimeoutMinutes,
// 			workerDMs[i].DirectoryName,
// 			workerDMs[i].Executable,
// 			workerDMs[i].ArgumentValues,
// 			[]*models.Schedule{},
// 		)
// 	}

// 	for i := range scheduleDMs {
// 		w := workerMap[scheduleDMs[i].WorkerID]
// 		log.Printf("Looking up worker %d for schedule %d", scheduleDMs[i].WorkerID, scheduleDMs[i].ID)
// 		w.Schedules = append(w.Schedules, models.NewSchedule(
// 			scheduleDMs[i].ID,
// 			scheduleDMs[i].IsActive,
// 			scheduleDMs[i].WorkerID,
// 			scheduleDMs[i].Sunday,
// 			scheduleDMs[i].Monday,
// 			scheduleDMs[i].Tuesday,
// 			scheduleDMs[i].Wednesday,
// 			scheduleDMs[i].Thursday,
// 			scheduleDMs[i].Friday,
// 			scheduleDMs[i].Saturday,
// 			scheduleDMs[i].TimeOfDayUTC,
// 			scheduleDMs[i].RecurTime,
// 			scheduleDMs[i].RecurBetweenStartUTC,
// 			scheduleDMs[i].RecurBetweenEndUTC,
// 			scheduleDMs[i].OneTime,
// 		))
// 	}

// 	for wid := range workerMap {
// 		workers = append(workers, workerMap[wid])
// 	}

// 	return
// }

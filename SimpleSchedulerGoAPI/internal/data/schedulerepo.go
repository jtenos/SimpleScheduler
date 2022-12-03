package data

import (
	"context"
	"database/sql"
	"time"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
)

type ScheduleRepo struct {
	connStr string
}

func NewScheduleRepo(connStr string) ScheduleRepo {
	return ScheduleRepo{connStr}
}

func (r ScheduleRepo) Create(ctx context.Context, workerID int64, sunday bool,
	monday bool, tuesday bool, wednesday bool, thursday bool, friday bool, saturday bool,
	timeOfDayUTC *time.Time, recurTime *time.Time, recurBetweenStartUTC *time.Time,
	recurBetweenEndUTC *time.Time) (err error) {

	if !sunday && !monday && !tuesday && !wednesday && !thursday && !friday && !saturday {
		err = errorhandling.NewBadRequestError("you must select a day")
		return
	}
	if timeOfDayUTC != nil && recurTime != nil {
		err = errorhandling.NewBadRequestError("you must select only one of time of day or recurring")
		return
	}
	if timeOfDayUTC == nil && recurTime == nil {
		err = errorhandling.NewBadRequestError("you must select time of day or recurring")
		return
	}
	if recurBetweenStartUTC != nil && recurBetweenEndUTC != nil && recurBetweenStartUTC.After(*recurBetweenEndUTC) {
		err = errorhandling.NewBadRequestError("recur between times invalid")
		return
	}

	db, err := sql.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	_, err = db.ExecContext(ctx, "[app].[Schedules_Insert]",
		sql.Named("WorkerID", workerID),
		sql.Named("Sunday", sunday),
		sql.Named("Monday", monday),
		sql.Named("Tuesday", tuesday),
		sql.Named("Wednesday", wednesday),
		sql.Named("Thursday", thursday),
		sql.Named("Friday", friday),
		sql.Named("Saturday", saturday),
		sql.Named("TimeOfDayUTC", timeOfDayUTC),
		sql.Named("RecurTime", recurTime),
		sql.Named("RecurBetweenStartUTC", recurBetweenStartUTC),
		sql.Named("RecurBetweenEndUTC", recurBetweenEndUTC),
	)
	return
}

func (r ScheduleRepo) Delete(ctx context.Context, id int64) (err error) {
	db, err := sql.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	_, err = db.ExecContext(ctx, "[app].[Schedules_Deactivate]",
		sql.Named("ID", id),
	)
	return
}

/*


func (r WorkerRepo) Update(ctx context.Context, id int64, name string, description string,
	emailOnSuccess string, parentWorkerID *int64, timeoutMinutes int32, directory string,
	executable string, args string, workerPath string) (err error) {

	if !isValidExec(directory, executable, workerPath) {
		err = errors.New("invalid executable")
		return
	}

	if id == *parentWorkerID {
		err = errors.New("worker cannot be its own parent")
		return
	}

	db, err := sql.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	var success bool
	var nameAlreadyExists bool
	var circularReference bool

	row := db.QueryRowContext(ctx, "[app].[Workers_Update]",
		sql.Named("ID", id),
		sql.Named("WorkerName", name),
		sql.Named("DetailedDescription", description),
		sql.Named("EmailOnSuccess", emailOnSuccess),
		sql.Named("ParentWorkerID", parentWorkerID),
		sql.Named("TimeoutMinutes", timeoutMinutes),
		sql.Named("DirectoryName", directory),
		sql.Named("Executable", executable),
		sql.Named("ArgumentValues", args),
	)
	if err = row.Scan(&success, &nameAlreadyExists, &circularReference); err != nil {
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
	return
}


func (r WorkerRepo) Reactivate(ctx context.Context, id int64) (err error) {
	db, err := sql.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	_, err = db.ExecContext(ctx, "[app].[Workers_Reactivate]",
		sql.Named("ID", id),
	)
	return
}

func (r WorkerRepo) RunNow(ctx context.Context, id int64) (err error) {
	db, err := sql.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	_, err = db.ExecContext(ctx, "[app].[Jobs_RunNow]",
		sql.Named("ID", id),
	)
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

*/

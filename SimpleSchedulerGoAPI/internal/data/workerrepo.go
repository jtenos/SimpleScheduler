package data

import (
	"context"
	"database/sql"
	"log"
	"strings"

	"github.com/jmoiron/sqlx"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data/datamodels"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/models"
)

type WorkerRepo struct {
	connStr string
}

func NewWorkerRepo(connStr string) WorkerRepo {
	return WorkerRepo{connStr}
}

func (r WorkerRepo) Search(ctx context.Context, nameFilter string, directoryFilter string,
	executableFilter string, statusFilter string) (workers []*models.Worker, err error) {

	db, err := sqlx.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	activeOnly := strings.EqualFold(statusFilter, "active")
	inactiveOnly := strings.EqualFold(statusFilter, "inactive")

	/*
		Workers_Select:

		@ID BIGINT = NULL
		,@IDs NVARCHAR(MAX) = NULL -- JSON: [123,456]
		,@ParentWorkerID BIGINT = NULL
		,@WorkerName NVARCHAR(100) = NULL
		,@DirectoryName NVARCHAR(1000) = NULL
		,@Executable NVARCHAR(1000) = NULL
		,@ActiveOnly BIT = NULL
		,@InactiveOnly BIT = NULL

		Selects matching workers and then all matching schedules in second query
	*/

	var workerDMs []datamodels.Worker
	err = db.SelectContext(ctx, &workerDMs, "[app].[Workers_Select]",
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

	// TODO: Improve performance by filtering these schedules
	var scheduleDMs []datamodels.Schedule
	err = db.SelectContext(ctx, &scheduleDMs, "[app].[Schedules_SelectAll]",
		sql.Named("IncludeInactive", true),
	)
	if err != nil {
		return
	}

	log.Printf("Num scheduleDMs: %d", len(scheduleDMs))

	workerScheds := map[int64][]*datamodels.Schedule{}

	for i := range workerDMs {
		workerScheds[workerDMs[i].ID] = []*datamodels.Schedule{}
	}

	for i := range scheduleDMs {
		scheds, ok := workerScheds[scheduleDMs[i].WorkerID]
		if ok {
			scheds = append(scheds, &scheduleDMs[i])
			workerScheds[scheduleDMs[i].WorkerID] = scheds
		}
	}

	workers = make([]*models.Worker, len(workerDMs))

	for i := range workerDMs {
		wdm := workerDMs[i]

		schedDMs := workerScheds[wdm.ID]
		scheds := make([]*models.Schedule, len(schedDMs))
		for j := range schedDMs {
			s := schedDMs[j]
			scheds = append(scheds, models.NewSchedule(s.ID, s.IsActive, s.WorkerID,
				s.Sunday, s.Monday, s.Tuesday, s.Wednesday, s.Thursday, s.Friday, s.Saturday,
				s.TimeOfDayUTC, s.RecurTime, s.RecurBetweenStartUTC, s.RecurBetweenEndUTC, s.OneTime))
		}

		workers[i] = models.NewWorker(
			wdm.ID, wdm.IsActive, wdm.WorkerName, wdm.DetailedDescription, wdm.EmailOnSuccess,
			wdm.ParentWorkerID, wdm.TimeoutMinutes, wdm.DirectoryName, wdm.Executable,
			wdm.ArgumentValues, scheds,
		)
	}

	return
}

/*
func (r UserRepo) GetAllUserEmails(ctx context.Context) (emails []string, err error) {
	db, err := sqlx.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	var users []datamodels.User
	err = db.SelectContext(ctx, &users, "[app].[Users_SelectAll]")
	if err != nil {
		return
	}

	for i := range users {
		emails = append(emails, users[i].Email)
	}
	return
}

func (r UserRepo) SubmitEmail(ctx context.Context, email string) (valCd string, err error) {
	db, err := sqlx.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	var submitRes datamodels.SubmitLoginResult
	row := db.QueryRowxContext(ctx, "[app].[Users_SubmitLogin]", sql.Named("EmailAddress", email))
	err = row.StructScan(&submitRes)
	if err != nil {
		return
	}

	valCd = submitRes.ValidationCode.String()
	return
}

func (r UserRepo) ValidateEmail(ctx context.Context, valCd string) (success bool, email string, notFound bool, expired bool, err error) {
	db, err := sqlx.Open("sqlserver", r.connStr)
	if err != nil {
		return
	}
	defer db.Close()

	var valRes datamodels.ValidateLoginResult
	row := db.QueryRowxContext(ctx, "[app].[Users_ValidateLogin]", sql.Named("ValidationCode", valCd))
	err = row.StructScan(&valRes)
	if err != nil {
		return
	}

	success = valRes.Success
	if valRes.Email.Valid {
		email = valRes.Email.String
	}
	notFound = valRes.NotFound
	expired = valRes.Expired
	return
}

*/

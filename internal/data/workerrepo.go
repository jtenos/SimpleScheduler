package data

import (
	"context"
	"database/sql"
	"errors"
	"os"
	"path"
	"strconv"
	"strings"

	"github.com/jtenos/simplescheduler/internal/ctxutil"
	"github.com/jtenos/simplescheduler/internal/data/entity"
)

type WorkerRepo struct{ ctx context.Context }

func NewWorkerRepo(ctx context.Context) *WorkerRepo {
	return &WorkerRepo{ctx}
}

func (repo *WorkerRepo) GetByID(id int64) (*entity.WorkerEntity, error) {
	db, err := open(repo.ctx)
	if err != nil {
		return nil, err
	}
	defer db.Close()

	rows, err := db.QueryContext(repo.ctx, "SELECT * FROM [workers] WHERE [worker_id] = @worker_id;",
		sql.Named("worker_id", id),
	)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	if !rows.Next() {
		return nil, errors.New("worker not found")
	}
	var w entity.WorkerEntity
	err = w.Hydrate(rows)
	if err != nil {
		return nil, err
	}
	return &w, nil
}

func isValidExec(dir string, exe string, workerPath string) bool {
	if strings.Contains(dir, "/") || strings.Contains(dir, "\\") || strings.Contains(exe, "/") || strings.Contains(exe, "\\") {
		return false
	}

	fullPath := path.Join(workerPath, dir, exe)
	_, err := os.Lstat(fullPath)

	return err == nil
}

func (r *WorkerRepo) Create(ctx context.Context, name string, description string, emailOnSuccess string, parentWorkerID *int64,
	timeoutMinutes int32, directory string, executable string, args string) (err error) {

	if !isValidExec(directory, executable, ctxutil.GetWorkerPath(ctx)) {
		err = errors.New("invalid executable")
		return
	}

	existingWorkerNames, err := getWorkerNames(r.ctx)
	if err != nil {
		return err
	}

	for _, w := range existingWorkerNames {
		if strings.EqualFold(w, name) {
			return errors.New("worker name already exists")
		}
	}

	db, err := open(r.ctx)
	if err != nil {
		return err
	}
	defer db.Close()

	query := `INSERT INTO [workers] (
		 [worker_name], [detailed_description], [email_on_success], [parent_worker_id]
		,[timeout_minutes], [directory_name], [executable], [argument_values]
	) VALUES (
		 @worker_name, @detailed_description, @email_on_success, @parent_worker_id
		,@timeout_minutes, @directory_name, @executable, @argument_values
	);`

	_, err = db.ExecContext(r.ctx, query,
		sql.Named("worker_name", name),
		sql.Named("detailed_description", description),
		sql.Named("email_on_success", emailOnSuccess),
		sql.Named("parent_worker_id", parentWorkerID),
		sql.Named("timeout_minutes", timeoutMinutes),
		sql.Named("directory_name", directory),
		sql.Named("executable", executable),
		sql.Named("argument_values", args),
	)

	return err
}

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

func getWorkerNames(ctx context.Context) ([]string, error) {
	db, err := open(ctx)
	if err != nil {
		return nil, err
	}
	defer db.Close()

	const query = "SELECT [worker_name] FROM [workers];"
	rows, err := db.QueryContext(ctx, query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var names []string

	for rows.Next() {
		var name string
		err = rows.Scan(&name)
		if err != nil {
			return nil, err
		}
		names = append(names, name)
	}

	return names, nil
}

func (repo *WorkerRepo) Search(ctx context.Context, nameFilter string, descFilter string, dirFilter string,
	exeFilter string, activeFilter string, parentFilter string) ([]*entity.WorkerEntity, error) {

	db, err := open(repo.ctx)
	if err != nil {
		return nil, err
	}
	defer db.Close()

	args := []sql.NamedArg{}

	query := " SELECT * FROM [workers] WHERE 1 = 1 "

	if len(nameFilter) > 0 {
		query += " AND [worker_name] LIKE '%' || @worker_name || '%' "
		args = append(args, sql.Named("worker_name", nameFilter))
	}
	if len(descFilter) > 0 {
		query += " AND [detailed_description] LIKE '%' || @detailed_description || '%' "
		args = append(args, sql.Named("detailed_description", descFilter))
	}
	if len(dirFilter) > 0 {
		query += " AND [directory_name] LIKE '%' || @directory_name || '%' "
		args = append(args, sql.Named("directory_name", dirFilter))
	}
	if len(exeFilter) > 0 {
		query += " AND [executable] LIKE '%' || @executable || '%' "
	}

	switch activeFilter {
	case "0":
		query += " AND [is_active] = 0 "
	case "1":
		query += " AND [is_active] = 1 "
	default:
		break
	}

	if len(parentFilter) > 0 {
		pid, err := strconv.ParseInt(parentFilter, 10, 64)
		if err == nil {
			query += " AND [parent_worker_id] = @parent_worker_id "
			args = append(args, sql.Named("parent_worker_id", pid))
		}
	}

	rows, err := db.QueryContext(repo.ctx, query, args)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	workers := []*entity.WorkerEntity{}

	for rows.Next() {
		var w entity.WorkerEntity
		err = w.Hydrate(rows)
		if err != nil {
			return nil, err
		}
		workers = append(workers, &w)
	}

	return workers, nil
}

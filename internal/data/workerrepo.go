package data

import (
	"context"
	"database/sql"
	"errors"
	"fmt"
	"os"
	"path"
	"strconv"
	"strings"
	"time"

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

func (r *WorkerRepo) Create(name string, description string, emailOnSuccess string, parentWorkerID *int64,
	timeoutMinutes int64, directory string, executable string, args string) (int64, error) {

	if !isValidExec(directory, executable, ctxutil.GetWorkerPath(r.ctx)) {
		err := errors.New("invalid executable")
		return 0, err
	}

	existingWorkerNames, err := getWorkerNames(r.ctx)
	if err != nil {
		return 0, err
	}

	for _, w := range existingWorkerNames {
		if strings.EqualFold(w, name) {
			return 0, errors.New("worker name already exists")
		}
	}

	db, err := open(r.ctx)
	if err != nil {
		return 0, err
	}
	defer db.Close()

	query := `INSERT INTO [workers] (
		 [worker_name], [detailed_description], [email_on_success], [parent_worker_id]
		,[timeout_minutes], [directory_name], [executable], [argument_values]
	) VALUES (
		 @worker_name, @detailed_description, @email_on_success, @parent_worker_id
		,@timeout_minutes, @directory_name, @executable, @argument_values
	);
	SELECT last_insert_rowid()`

	row := db.QueryRowContext(r.ctx, query,
		sql.Named("worker_name", name),
		sql.Named("detailed_description", description),
		sql.Named("email_on_success", emailOnSuccess),
		sql.Named("parent_worker_id", parentWorkerID),
		sql.Named("timeout_minutes", timeoutMinutes),
		sql.Named("directory_name", directory),
		sql.Named("executable", executable),
		sql.Named("argument_values", args),
	)

	var workerID int64
	err = row.Scan(&workerID)
	if err != nil {
		return 0, err
	}

	return workerID, nil
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

func (r WorkerRepo) Delete(id int64) (err error) {
	db, err := open(r.ctx)
	if err != nil {
		return
	}
	defer db.Close()

	w, err := r.GetByID(id)
	if err != nil {
		return
	}

	formattedNow := time.Now().UTC().Format("20060102150405")
	newName := fmt.Sprintf("INACTIVE: %s %s", formattedNow, w.WorkerName)

	if len(newName) > 100 {
		runes := []rune(newName)
		newName = string(runes[0:100])
	}

	query := `
		UPDATE [schedules] SET [is_active] = 0
		WHERE [worker_id] = @worker_id;
		UPDATE [workers] SET [is_active] = 0, [worker_name] = @worker_name]
		WHERE [id] = @worker_id;
	`

	_, err = db.ExecContext(r.ctx, query,
		sql.Named("worker_id", id),
		sql.Named("worker_name", newName),
	)
	return
}

/*
		DECLARE @WorkerName NVARCHAR(100);
		SELECT @WorkerName = [WorkerName] FROM [app].[Workers] WHERE [ID] = @ID;

		DECLARE @NewWorkerName NVARCHAR(150) = N'INACTIVE: ' + FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmss')
			+ ' ' + @WorkerName;

		SET @NewWorkerName = LTRIM(RTRIM(LEFT(@NewWorkerName, 100)));

		UPDATE [app].[Workers]
		SET [IsActive] = 0, [WorkerName] = @NewWorkerName
		WHERE [ID] = @ID;

		DELETE j
		FROM [app].[Jobs] j
		JOIN [app].[Schedules] s ON j.[ScheduleID] = s.[ID]
		WHERE s.[WorkerID] = @ID
		AND j.[StatusCode] = 'NEW';

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		DECLARE @Msg NVARCHAR(2048) = ERROR_MESSAGE();
		RAISERROR(@Msg, 16, 1);
		RETURN 55555;
	END CATCH;
END;
GO

-- TODO: Move this into regular Update proc

*/

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

func (repo *WorkerRepo) Search(nameFilter string, descFilter string, dirFilter string,
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

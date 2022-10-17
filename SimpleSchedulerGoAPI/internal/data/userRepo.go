package data

import (
	"context"
	"database/sql"

	"github.com/jmoiron/sqlx"
	_ "github.com/microsoft/go-mssqldb"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/datamodels"
)

type UserRepo struct {
	connStr string
}

func NewUserRepo(connStr string) UserRepo {
	return UserRepo{connStr}
}

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
		valCd = ""
		return
	}
	defer db.Close()

	var submitResp datamodels.SubmitLoginResult
	row := db.QueryRowxContext(ctx, "[app].[Users_SubmitLogin]", sql.Named("EmailAddress", email))
	err = row.StructScan(&submitResp)
	if err != nil {
		valCd = ""
		return
	}

	valCd = submitResp.ValidationCode.String()
	return
}

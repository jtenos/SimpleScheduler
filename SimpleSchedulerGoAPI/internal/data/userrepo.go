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

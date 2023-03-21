package data

import (
	"context"
	"database/sql"
	"strings"

	"github.com/google/uuid"
	"github.com/jtenos/simplescheduler/internal/datamodels"
)

type UserRepo struct{ ctx context.Context }

func NewUserRepo(ctx context.Context) *UserRepo {
	return &UserRepo{ctx}
}

func (repo *UserRepo) GetUserEmailAddresses() ([]string, error) {
	db, err := open(repo.ctx)
	if err != nil {
		return nil, err
	}
	defer db.Close()

	rows, err := db.QueryContext(repo.ctx, "SELECT [email_address] FROM [users];")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var result []string

	for rows.Next() {
		var u datamodels.User
		err = u.Hydrate(rows)
		if err != nil {
			return nil, err
		}
		result = append(result, u.Email)
	}

	return result, nil
}

func (repo *UserRepo) SubmitEmail(email string) (userFound bool, valCd string, err error) {
	db, err := open(repo.ctx)
	if err != nil {
		return
	}
	defer db.Close()

	rows, err := db.QueryContext(repo.ctx, "SELECT [email_address] FROM [users] WHERE [email_address] = @email_address;",
		sql.Named("email_address", email),
	)
	if err != nil {
		return
	}
	defer rows.Close()
	if !rows.Next() {
		return
	}

	var u datamodels.User
	err = u.Hydrate(rows)
	if err != nil {
		return
	}

	userFound = true

	valCd = strings.ReplaceAll(uuid.New().String(), "-", "")

	_, err = db.ExecContext(repo.ctx, `
		INSERT INTO [login_attempts] (
			[submit_date_utc], [email_address], [validation_code]
		) VALUES (
			@submit_date_utc, @email_address, @validation_code
		);
		`,
		sql.Named("submit_date_utc", getFormattedUtcNow()),
		sql.Named("email_address", u.Email),
		sql.Named("validation_code", valCd),
	)

	if err != nil {
		return
	}

	return
}

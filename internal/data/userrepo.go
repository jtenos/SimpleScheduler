package data

import (
	"context"
	"database/sql"
	"errors"

	"github.com/jtenos/simplescheduler/internal/data/entity"
	"github.com/jtenos/simplescheduler/internal/util"
)

type UserRepo struct{ ctx context.Context }

func NewUserRepo(ctx context.Context) *UserRepo {
	return &UserRepo{ctx}
}

type UserNotFoundError struct{}

func (e *UserNotFoundError) Error() string {
	return "user not found"
}

func (repo *UserRepo) GetUserEmailAddresses() ([]entity.UserEntity, error) {
	db, err := open(repo.ctx)
	if err != nil {
		return nil, err
	}
	defer db.Close()

	rows, err := db.QueryContext(repo.ctx, "SELECT * FROM [users];")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var result []entity.UserEntity

	for rows.Next() {
		var u entity.UserEntity
		err = u.Hydrate(rows)
		if err != nil {
			return nil, err
		}
		result = append(result, u)
	}

	return result, nil
}

func (repo *UserRepo) SubmitEmail(emailAddress string) (string, error) {
	db, err := open(repo.ctx)
	if err != nil {
		return "", err
	}
	defer db.Close()

	rows, err := db.QueryContext(repo.ctx, "SELECT * FROM [users] WHERE [email_address] = @email_address;",
		sql.Named("email_address", emailAddress),
	)
	if err != nil {
		return "", err
	}
	defer rows.Close()
	if !rows.Next() {
		return "", errors.New("user not found")
	}

	var u entity.UserEntity
	err = u.Hydrate(rows)
	if err != nil {
		return "", err
	}

	valCd := util.UuidLower()

	_, err = db.ExecContext(repo.ctx, `
		INSERT INTO [login_attempts] ([submit_date_utc], [email_address], [validation_code])
		VALUES (@submit_date_utc, @email_address, @validation_code);`,
		sql.Named("submit_date_utc", getFormattedUtcNow()),
		sql.Named("email_address", u.EmailAddress),
		sql.Named("validation_code", valCd),
	)

	if err != nil {
		return "", err
	}

	return valCd, nil
}

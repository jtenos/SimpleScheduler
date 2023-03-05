package data

import (
	"context"
	"database/sql"
	"strings"

	"github.com/google/uuid"
	"jtenos.com/simplescheduler/internal/datamodels"
)

type UserRepo struct{ ctx context.Context }

func NewUserRepo(ctx context.Context) *UserRepo {
	return &UserRepo{ctx}
}

func (repo *UserRepo) SubmitEmail(email string) (userFound bool, valCd string, err error) {
	db, err := open(repo.ctx)
	if err != nil {
		return
	}
	defer db.Close()

	rows, err := db.QueryContext(repo.ctx, "SELECT * FROM [users] WHERE [email_address] = @email_address;",
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

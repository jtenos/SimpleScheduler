package data

import (
	"context"

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
	err = db.Select(&users, "[app].[Users_SelectAll]")
	//rows, err := db.QueryContext(ctx, "[app].[Users_SelectAll]")
	if err != nil {
		return
	}

	for i := range users {
		emails = append(emails, users[i].EmailAddress)
	}

	// defer rows.Close()
	// for rows.Next() {
	// 	u := new(datamodels.User)
	// 	rows.Scan(&u)
	// 	emails = append(emails, u.EmailAddress)
	// 	log.Printf("user: %s\n", u.EmailAddress)
	// }
	return
}

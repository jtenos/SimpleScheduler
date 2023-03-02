package datamodels

import "database/sql"

// [app].[Users] table
type User struct {
	Email string `db:"EmailAddress"`
}

func (u *User) Hydrate(rows *sql.Rows) error {
	return rows.Scan(&u.Email)
}

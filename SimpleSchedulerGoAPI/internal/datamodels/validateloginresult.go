package datamodels

import "database/sql"

type ValidateLoginResult struct {
	Success  bool           `db:"Success"`
	Email    sql.NullString `db:"EmailAddress"`
	NotFound bool           `db:"NotFound"`
	Expired  bool           `db:"Expired"`
}

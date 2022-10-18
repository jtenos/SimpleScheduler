package datamodels

import mssql "github.com/microsoft/go-mssqldb"

type SubmitLoginResult struct {
	Success        bool                   `db:"Success"`
	ValidationCode mssql.UniqueIdentifier `db:"ValidationCode"`
}

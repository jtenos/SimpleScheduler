package data

// import (
// 	"context"
// 	"database/sql"
// )

// type UserRepo struct {
// 	connStr string
// }

// func NewUserRepo(connStr string) UserRepo {
// 	return UserRepo{connStr}
// }

// func (r UserRepo) GetAllUserEmails(ctx context.Context) (emails []string, err error) {
// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	var users []datamodels.User
// 	rows, err := db.QueryContext(ctx, "[app].[Users_SelectAll]")
// 	if err != nil {
// 		return
// 	}
// 	defer rows.Close()
// 	var u datamodels.User
// 	for rows.Next() {
// 		if err = u.Hydrate(rows); err == nil {
// 			return
// 		}
// 		users = append(users, u)
// 	}

// 	for i := range users {
// 		emails = append(emails, users[i].Email)
// 	}
// 	return
// }

// func (r UserRepo) SubmitEmail(ctx context.Context, email string) (valCd string, err error) {
// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	var success bool
// 	var valCdGuid mssql.UniqueIdentifier
// 	row := db.QueryRowContext(ctx, "[app].[Users_SubmitLogin]", sql.Named("EmailAddress", email))
// 	err = row.Scan(&success, &valCdGuid)
// 	if err != nil {
// 		return
// 	}

// 	valCd = valCdGuid.String()
// 	return
// }

// func (r UserRepo) ValidateEmail(ctx context.Context, valCd string) (success bool, email string, notFound bool, expired bool, err error) {
// 	db, err := sql.Open("sqlserver", r.connStr)
// 	if err != nil {
// 		return
// 	}
// 	defer db.Close()

// 	row := db.QueryRowContext(ctx, "[app].[Users_ValidateLogin]", sql.Named("ValidationCode", valCd))
// 	err = row.Scan(&success, &email, &notFound, &expired)
// 	return
// }

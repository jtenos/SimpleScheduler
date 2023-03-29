package ctxutil

import "context"

// type nounKey struct{}
// type verbKey struct{}
// type tokenKey struct{}
// type verboseKey struct{}

type allowLoginDropDownKey struct{}
type apiUrlKey struct{}
type dbFileNameKey struct{}
type workerPathKey struct{}

/********** Allow Login Dropdown *********************/
func SetAllowLoginDropDown(ctx *context.Context, val bool) {
	*ctx = context.WithValue(*ctx, allowLoginDropDownKey{}, val)
}

func GetAllowLoginDropDown(ctx context.Context) bool {
	val, _ := ctx.Value(allowLoginDropDownKey{}).(bool)
	return val
}

/*****************************************************/

/***************** API URL ***************************/
func SetApiUrl(ctx *context.Context, apiUrl string) {
	*ctx = context.WithValue(*ctx, apiUrlKey{}, apiUrl)
}

func GetApiUrl(ctx context.Context) string {
	apiUrl, _ := ctx.Value(apiUrlKey{}).(string)
	return apiUrl
}

/*****************************************************/

/************* DB FileName ***************************/
func SetDBFileName(ctx *context.Context, dbFileName string) {
	*ctx = context.WithValue(*ctx, dbFileNameKey{}, dbFileName)
}

func GetDBFileName(ctx context.Context) string {
	dbFileName, _ := ctx.Value(dbFileNameKey{}).(string)
	return dbFileName
}

/*****************************************************/

/********* Worker Path *******************************/
func SetWorkerPath(ctx *context.Context, val string) {
	*ctx = context.WithValue(*ctx, workerPathKey{}, val)
}

func GetWorkerPath(ctx context.Context) string {
	val, _ := ctx.Value(workerPathKey{}).(string)
	return val
}

/*****************************************************/

// func SetNoun(ctx *context.Context, noun string) {
// 	*ctx = context.WithValue(*ctx, nounKey{}, noun)
// }

// func GetNoun(ctx context.Context) string {
// 	noun, _ := ctx.Value(nounKey{}).(string)
// 	return noun
// }

// func SetVerb(ctx *context.Context, verb string) {
// 	*ctx = context.WithValue(*ctx, verbKey{}, verb)
// }

// func GetVerb(ctx context.Context) string {
// 	verb, _ := ctx.Value(verbKey{}).(string)
// 	return verb
// }

// func SetToken(ctx *context.Context, token string) {
// 	*ctx = context.WithValue(*ctx, tokenKey{}, token)
// }

// func GetToken(ctx context.Context) string {
// 	token, _ := ctx.Value(tokenKey{}).(string)
// 	return token
// }

// func SetVerbose(ctx *context.Context, verbose bool) {
// 	*ctx = context.WithValue(*ctx, verboseKey{}, verbose)
// }

// func GetVerbose(ctx context.Context) bool {
// 	verbose, _ := ctx.Value(verboseKey{}).(bool)
// 	return verbose
// }

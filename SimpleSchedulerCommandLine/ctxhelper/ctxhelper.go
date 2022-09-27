package ctxhelper

import "context"

type nounKey struct{}
type verbKey struct{}
type apiUrlKey struct{}
type tokenKey struct{}

func SetNoun(ctx *context.Context, noun string) {
	*ctx = context.WithValue(*ctx, nounKey{}, noun)
}

func GetNoun(ctx context.Context) string {
	noun, _ := ctx.Value(nounKey{}).(string)
	return noun
}

func SetVerb(ctx *context.Context, verb string) {
	*ctx = context.WithValue(*ctx, verbKey{}, verb)
}

func GetVerb(ctx context.Context) string {
	verb, _ := ctx.Value(verbKey{}).(string)
	return verb
}

func SetApiUrl(ctx *context.Context, apiUrl string) {
	*ctx = context.WithValue(*ctx, apiUrlKey{}, apiUrl)
}

func GetApiUrl(ctx context.Context) string {
	apiUrl, _ := ctx.Value(apiUrlKey{}).(string)
	return apiUrl
}

func SetToken(ctx *context.Context, token string) {
	*ctx = context.WithValue(*ctx, tokenKey{}, token)
}

func GetToken(ctx context.Context) string {
	token, _ := ctx.Value(tokenKey{}).(string)
	return token
}

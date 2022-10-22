package security

import (
	"context"
	"encoding/json"
	"net/http"
	"time"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/jwt"
)

type ValidateTokenHandler struct {
	ctx    context.Context
	jwtKey []byte
}

func NewValidateTokenHandler(ctx context.Context, jwtKey []byte) *ValidateTokenHandler {
	return &ValidateTokenHandler{ctx, jwtKey}
}

type validateTokenReply struct {
	Success      bool      `json:"success"`
	Email        string    `json:"email"`
	Expires      time.Time `json:"expires"`
	ErrorMessage string    `json:"errorMessage"`
}

func (h *ValidateTokenHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	val := r.Context().Value(jwt.EmailClaimKey{})
	if val == nil {
		json.NewEncoder(w).Encode(validateTokenReply{false, "", time.Time{}, "not logged in"})
		return
	}
	email := val.(string)
	if len(email) == 0 {
		json.NewEncoder(w).Encode(validateTokenReply{false, "", time.Time{}, "not logged in"})
		return
	}
	val = r.Context().Value(jwt.TokenExpiresKey{})
	if val == nil {
		json.NewEncoder(w).Encode(validateTokenReply{false, "", time.Time{}, "invalid expiration date"})
		return
	}
	expires := val.(time.Time)

	json.NewEncoder(w).Encode(validateTokenReply{true, email, expires, ""})
}

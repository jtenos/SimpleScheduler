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
	tokenStr := r.URL.Query().Get("token")
	email, expires, err := jwt.ReadToken(h.jwtKey, tokenStr)
	if err != nil {
		json.NewEncoder(w).Encode(validateTokenReply{false, "", time.Time{}, err.Error()})
		return
	}
	json.NewEncoder(w).Encode(validateTokenReply{true, email, expires, ""})
}

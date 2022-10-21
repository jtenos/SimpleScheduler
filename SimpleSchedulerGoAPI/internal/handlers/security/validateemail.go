package security

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/errors"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/jwt"
)

type ValidateEmailHandler struct {
	ctx     context.Context
	connStr string
	jwtKey  []byte
}

func NewValidateEmailHandler(ctx context.Context, connStr string, jwtKey []byte) *ValidateEmailHandler {
	return &ValidateEmailHandler{ctx, connStr, jwtKey}
}

type validateEmailReply struct {
	Jwt string `json:"jwt"`
}

func (h *ValidateEmailHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	valCd := r.URL.Query().Get("cd")
	if len(valCd) == 0 {
		errors.HandleError(w, r, http.StatusBadRequest, "cd parameter is required", true)
		return
	}

	userRepo := data.NewUserRepo(h.connStr)
	success, email, notFound, expired, err := userRepo.ValidateEmail(h.ctx, valCd)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error(), false)
		return
	}
	if notFound {
		errors.HandleError(w, r, http.StatusBadRequest, "Validation code not found", true)
		return
	}
	if expired {
		errors.HandleError(w, r, http.StatusBadRequest, "Validation code expired", true)
		return
	}
	if len(email) == 0 {
		errors.HandleError(w, r, http.StatusInternalServerError, "email is empty", false)
		return
	}
	if !success {
		errors.HandleError(w, r, http.StatusInternalServerError, "Error calling ValidateEmail, success=false", false)
		return
	}

	token, err := jwt.CreateToken(h.jwtKey, email)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error(), false)
		return
	}

	json.NewEncoder(w).Encode(validateEmailReply{
		Jwt: token,
	})
}

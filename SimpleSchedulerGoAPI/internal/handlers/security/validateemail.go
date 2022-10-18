package security

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/errors"
)

type ValidateEmailHandler struct {
	ctx     context.Context
	connStr string
}

func NewValidateEmailHandler(ctx context.Context, connStr string) *ValidateEmailHandler {
	return &ValidateEmailHandler{ctx, connStr}
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

	json.NewEncoder(w).Encode(validateEmailReply{
		Jwt: "TODO: Insert Token Here",
	})
}

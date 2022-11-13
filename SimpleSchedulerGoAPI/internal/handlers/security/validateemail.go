package security

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/errorhandling"
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
		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("cd parameter is required"), "ValidateEmailHandler.ServeHTTP")
		return
	}

	userRepo := data.NewUserRepo(h.connStr)
	success, email, notFound, expired, err := userRepo.ValidateEmail(h.ctx, valCd)
	if err != nil {
		errorhandling.HandleError(w, r, err, "ValidateEmailHandler.ServeHTTP")
		return
	}
	if notFound {
		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("validation code not found"), "ValidateEmailHandler.ServeHTTP")
		return
	}
	if expired {
		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("validation code expired"), "ValidateEmailHandler.ServeHTTP")
		return
	}
	if len(email) == 0 {
		errorhandling.HandleError(w, r, errorhandling.NewBadRequestError("email is empty"), "ValidateEmailHandler.ServeHTTP")
		return
	}
	if !success {
		errorhandling.HandleError(w, r, errorhandling.NewInternalServerError("unknown error calling ValidateEmail, success=false"), "ValidateEmailHandler.ServeHTTP")
		return
	}

	token, err := jwt.CreateToken(h.jwtKey, email)
	if err != nil {
		errorhandling.HandleError(w, r, err, "ValidateEmailHandler.ServeHTTP")
		return
	}

	json.NewEncoder(w).Encode(validateEmailReply{
		Jwt: token,
	})
}

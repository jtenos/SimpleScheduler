package security

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/errors"
)

type GetAllUserEmailsHandler struct {
	ctx     context.Context
	connStr string
}

func NewGetAllUserEmailsHandler(ctx context.Context, connStr string) *GetAllUserEmailsHandler {
	return &GetAllUserEmailsHandler{ctx, connStr}
}

type getEmailsReply struct {
	Emails []string `json:"emails"`
}

func (h *GetAllUserEmailsHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	userRepo := data.NewUserRepo(h.connStr)
	emails, err := userRepo.GetAllUserEmails(h.ctx)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error(), false)
		return
	}
	json.NewEncoder(w).Encode(getEmailsReply{
		Emails: emails,
	})
}

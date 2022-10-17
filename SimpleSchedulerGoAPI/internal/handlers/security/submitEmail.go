package security

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/errors"
)

type SubmitEmailHandler struct {
	ctx     context.Context
	connStr string
	apiUrl  string
}

func NewSubmitEmailHandler(ctx context.Context, connStr string, apiUrl string) *SubmitEmailHandler {
	return &SubmitEmailHandler{ctx, connStr, apiUrl}
}

type submitEmailRequest struct {
	Email string `json:"email"`
}

type submitEmailReply struct {
	Success bool `json:"success"`
}

func (h *SubmitEmailHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	userRepo := data.NewUserRepo(h.connStr)

	var req submitEmailRequest

	err := json.NewDecoder(r.Body).Decode(&req)
	if err != nil {
		errors.HandleError(w, r, http.StatusBadRequest, err.Error())
		return
	}

	valCd, err := userRepo.SubmitEmail(h.ctx, req.Email)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error())
		return
	}

	url := fmt.Sprintf("%s/security/validateUser/%s", h.apiUrl, valCd)

	// TODO: Send the email
	log.Printf("URL: %s", url)

	json.NewEncoder(w).Encode(submitEmailReply{
		Success: true,
	})
}

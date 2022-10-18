package security

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"strings"

	"github.com/google/uuid"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/data"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/emailer"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/errors"
)

type SubmitEmailHandler struct {
	ctx     context.Context
	connStr string
	apiUrl  string
	envName string
}

func NewSubmitEmailHandler(ctx context.Context, connStr string, apiUrl string, envName string) *SubmitEmailHandler {
	return &SubmitEmailHandler{ctx, connStr, apiUrl, envName}
}

type submitEmailReply struct {
	Success bool `json:"success"`
}

func (h *SubmitEmailHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {

	email := r.URL.Query().Get("email")
	if len(email) == 0 {
		errors.HandleError(w, r, http.StatusBadRequest, "email parameter is required", true)
		return
	}

	userRepo := data.NewUserRepo(h.connStr)

	valCd, err := userRepo.SubmitEmail(h.ctx, email)
	if err != nil {
		errors.HandleError(w, r, http.StatusInternalServerError, err.Error(), false)
		return
	}
	if valCd == uuid.Nil.String() {
		errors.HandleError(w, r, http.StatusBadRequest, fmt.Errorf("user not found").Error(), true)
		return
	}

	url := fmt.Sprintf("%s/security/validateEmail?cd=%s", h.apiUrl, valCd)
	body := new(strings.Builder)
	fmt.Fprintf(body, "<a href='%s' target=_blank>Click here to log in</a><br><br>", url)
	fmt.Fprint(body, "Or copy and paste the following:<br><br>")
	fmt.Fprint(body, valCd)
	fmt.Fprint(body, "<br>")

	emailer.SendEmail([]string{email}, "Log In", body.String())

	json.NewEncoder(w).Encode(submitEmailReply{
		Success: true,
	})
}

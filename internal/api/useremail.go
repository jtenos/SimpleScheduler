package api

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"html/template"
	"net/http"

	"github.com/jtenos/simplescheduler/internal/api/errorhandling"
	"github.com/jtenos/simplescheduler/internal/ctxutil"
	"github.com/jtenos/simplescheduler/internal/data"
	"github.com/jtenos/simplescheduler/internal/emailer"
	"github.com/julienschmidt/httprouter"
)

type UserEmailHandler struct {
	ctx  context.Context
	tmpl *template.Template
}

func NewUserEmailHandler(ctx context.Context) *UserEmailHandler {
	tmpl, _ := template.New("UserEmail").Parse(`
	<a href='{{.Url}}' target=_blank>Click here to log in</a>
	<br><br>
	Or copy and paste the following:
	<br><br>
	{{.ValCd}}
	<br>
	`)
	return &UserEmailHandler{ctx, tmpl}
}

func (h *UserEmailHandler) Get(w http.ResponseWriter, r *http.Request, _ httprouter.Params) {
	if !ctxutil.GetAllowLoginDropDown(h.ctx) {
		fmt.Fprint(w, "[]")
		return
	}

	userRepo := data.NewUserRepo(h.ctx)
	userEmails, err := userRepo.GetUserEmailAddresses()
	if err != nil {
		errorhandling.HandleError(w, r, err, "UserEmailHandler.Get", http.StatusInternalServerError)
	}
	json.NewEncoder(w).Encode(userEmails)
}

func (h *UserEmailHandler) Post(w http.ResponseWriter, r *http.Request, _ httprouter.Params) {

	type userEmailReply struct {
		Success bool `json:"success"`
	}

	decoder := json.NewDecoder(r.Body)
	var em struct {
		Email string `json:"email"`
	}
	err := decoder.Decode(&em)
	if err != nil {
		errorhandling.HandleError(w, r,
			errors.New("body must be JSON {\"email\":\"test@example.com\"}"),
			"UserEmailHandler.Post",
			http.StatusBadRequest,
		)
		return
	}

	userRepo := data.NewUserRepo(h.ctx)
	valCd, err := userRepo.SubmitEmail(em.Email)

	if _, ok := err.(*data.UserNotFoundError); ok {
		errorhandling.HandleError(w, r,
			errors.New("user not found"),
			"UserEmailHandler.Post",
			http.StatusNotFound,
		)
		return
	}

	if err != nil {
		errorhandling.HandleError(w, r, err, "UserEmailHandler.Post", http.StatusInternalServerError)
		return
	}

	url := fmt.Sprintf("%s/uservalidation/%s", ctxutil.GetApiUrl(h.ctx), valCd)

	var bodyBuf bytes.Buffer
	h.tmpl.Execute(&bodyBuf, struct {
		Url   string
		ValCd string
	}{
		Url:   url,
		ValCd: valCd,
	})

	emailer.SendEmail([]string{em.Email}, "Log In", bodyBuf.String())

	fmt.Fprint(w, "{}")
}

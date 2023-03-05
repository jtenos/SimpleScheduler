package api

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/julienschmidt/httprouter"
	"jtenos.com/simplescheduler/internal/api/errorhandling"
	"jtenos.com/simplescheduler/internal/data"
)

type UserEmailHandler struct {
	ctx context.Context
}

func NewUserEmailHandler(ctx context.Context) *UserEmailHandler {
	return &UserEmailHandler{ctx}
}

func (h *UserEmailHandler) Post(w http.ResponseWriter, r *http.Request, _ httprouter.Params) {

	decoder := json.NewDecoder(r.Body)
	var em struct {
		Email string `json:"email"`
	}
	err := decoder.Decode(&em)
	if err != nil {
		errorhandling.HandleError(w, r,
			errorhandling.NewBadRequestError("body must be JSON {\"email\":\"test@example.com\"}"),
			"UserEmailHandler.Post",
		)
		return
	}

	userRepo := data.NewUserRepo(h.ctx)
	userFound, valCd, err := userRepo.SubmitEmail(em.Email)
	if err != nil {
		errorhandling.HandleError(w, r, err, "UserEmailHandler.Post")
		return
	}
	if !userFound {
		errorhandling.HandleError(w, r,
			errorhandling.NewNotFoundError("user not found"),
			"UserEmailHandler.Post",
		)
		return
	}

	/*
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

	*/
}

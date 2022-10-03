package user

import (
	"context"
	"flag"
	"fmt"
	"strings"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/api"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxutil"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/userdir"
)

func Execute(ctx context.Context) {

	verb := ctxutil.GetVerb(ctx)

	switch verb {
	case "login":
		login(ctx)
	case "validate":
		validate(ctx)
	default:
		ui.WriteFatalf("Invalid verb: Must be 'login' or 'validate'")
	}
}

func login(ctx context.Context) {
	var email string
	flag.StringVar(&email, "email", "", "Email Address")
	flag.Parse()
	email = strings.TrimSpace(email)
	if email == "" {
		flag.PrintDefaults()
		ui.WriteFatalf("--email is required")
	}

	type request struct {
		EmailAddress string `json:"emailAddress"`
	}
	type reply struct {
		Success bool `json:"success"`
	}

	req := request{email}
	rep := &reply{}
	err := api.Post(ctx, "Login/SubmitEmail", req, rep)

	if err != nil {
		ui.WriteFatalf(err.Error())
	}

	if !rep.Success {
		ui.WriteFatalf("Login failed")
	}

	fmt.Println("Please check your email for a login code.")
	fmt.Println("Upon return:")
	fmt.Println("sched user validate --code abcdabcd-0123-4567-8910-1234567890ab")
}

func validate(ctx context.Context) {
	var code string
	flag.StringVar(&code, "code", "", "Validation Code")
	flag.Parse()
	code = strings.TrimSpace(code)
	if code == "" {
		flag.PrintDefaults()
		ui.WriteFatalf("--code is required")
	}

	type request struct {
		ValidationCode string `json:"validationCode"`
	}
	type reply struct {
		JwtToken string `json:"jwtToken"`
	}

	req := request{code}
	rep := &reply{}
	err := api.Post(ctx, "Login/ValidateEmail", req, rep)

	if err != nil {
		ui.WriteFatalf(err.Error())
	}

	jwt := rep.JwtToken
	err = userdir.WriteToken(jwt)
	if err != nil {
		ui.WriteFatalf("Error writing token: %s", err.Error())
	}

	fmt.Println("User has been authenticated. You may proceed with commands.")
}

package user

import (
	"context"
	"flag"
	"fmt"
	"log"
	"strings"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/api"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/apimodels"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/userdir"
)

func Execute(ctx context.Context) {

	verb := ctxhelper.GetVerb(ctx)

	switch verb {
	case "login":
		login(ctx)
	case "validate":
		validate(ctx)
	default:
		log.Fatal("Invalid verb: Must be 'login' or 'validate'")
	}
}

func login(ctx context.Context) {
	var email string
	flag.StringVar(&email, "email", "", "Email Address")
	flag.Parse()
	email = strings.TrimSpace(email)
	if email == "" {
		flag.PrintDefaults()
		log.Fatal("--email is required")
	}
	req := apimodels.NewSubmitEmailRequest(email)
	rep := apimodels.NewSubmitEmailReply()
	err := api.Post(ctx, "Login/SubmitEmail", req, rep)

	if err != nil {
		log.Fatal(err.Error())
	}

	if !rep.Success {
		log.Fatal("Login failed")
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
		log.Fatal("--code is required")
	}

	req := apimodels.NewValidateEmailRequest(code)
	rep := apimodels.NewValidateEmailReply()
	err := api.Post(ctx, "Login/ValidateEmail", req, rep)

	if err != nil {
		log.Fatal(err.Error())
	}

	jwt := rep.JwtToken
	err = userdir.WriteToken(jwt)
	if err != nil {
		log.Fatalf("Error writing token: %s", err.Error())
	}

	fmt.Println("User has been authenticated. You may proceed with commands.")
}

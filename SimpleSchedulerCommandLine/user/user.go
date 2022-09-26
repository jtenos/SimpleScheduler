package user

import (
	"context"
	"flag"
	"fmt"
	"log"
	"strings"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/api"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/apimodels"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/contextkeys"
)

func Execute(ctx context.Context) {

	verb := ctx.Value(contextkeys.VerbKey{})

	switch verb {
	case "login":
		login()
	case "validate":
		validate()
	}
}

func login() {
	email := flag.String("email", "", "Email Address")
	flag.Parse()
	fmt.Printf("email=%s\n", *email)
	if len(strings.TrimSpace(*email)) == 0 {
		flag.PrintDefaults()
		//ui.WriteHelp(1)
		log.Fatal("")
	}
	req := apimodels.NewSubmitEmailRequest(*email)
	rep := apimodels.NewSubmitEmailReply()
	err := api.Post("Login/SubmitEmail", req, rep)

	if err != nil {
		log.Fatal(err.Error())
	}

	if !rep.Success {
		log.Fatal("Login failed")
	}

	fmt.Println("Please check your email for a login code.")
}

func validate() {

}

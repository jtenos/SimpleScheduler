package ui

import (
	"bufio"
	"fmt"
	"os"
	"strings"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/api"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/apireply"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/apirequest"
)

var rdr = bufio.NewReader(os.Stdin)

var apiUrl string

func Initialize(baseUrl string) {
	apiUrl = baseUrl
}

func ShowHeader() {
	fmt.Print("\n*** SIMPLE SCHEDULER ***\n\n")
}

func LogIn() (*apireply.ValidateEmailReply, error) {
	fmt.Print("E-Mail Address: ")

	email := readFromConsole()

	fmt.Println("Submitting to API, please wait...")

	api := api.NewApiClient(apiUrl)
	req := apirequest.NewSubmitEmailRequest(email)
	rep := apireply.NewSubmitEmailReply()
	err := api.Post("Login/SubmitEmail", req, rep)

	if err != nil {
		return nil, err
	}

	if !rep.Success {
		fmt.Println("Login failed. Please try again.")
		fmt.Println("")
		return LogIn()
	}

	fmt.Println("Please check your email for a login code.")
	fmt.Println("")
	fmt.Print("Enter login code: ")
	loginCode := readFromConsole()

	req2 := apirequest.NewValidateEmailRequest(loginCode)
	rep2 := apireply.NewValidateEmailReply()
	err = api.Post("Login/ValidateEmail", req2, rep2)

	if err != nil {
		return nil, err
	}

	return rep2, nil
}

func readFromConsole() string {
	text, _ := rdr.ReadString('\n')
	text = strings.TrimSpace(text)
	return text
}

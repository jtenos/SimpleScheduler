package ui

import (
	"bufio"
	"fmt"
	"log"
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

func LogIn() {
	fmt.Print("E-Mail Address: ")

	email := readFromConsole()

	fmt.Println("Submitting to API, please wait...")

	api := api.NewApiClient(apiUrl)
	req := apirequest.NewSubmitEmailRequest(email)
	rep := apireply.NewSubmitEmailReply()
	err := api.Post("Login/SubmitEmail", req, rep)

	if err != nil {
		log.Fatal(err)
	}

	fmt.Printf("Success is %v\n", rep.Success)
}

func readFromConsole() string {
	text, _ := rdr.ReadString('\n')
	text = strings.TrimSpace(text)
	return text
}

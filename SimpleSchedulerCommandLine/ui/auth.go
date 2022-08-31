package ui

import (
	"fmt"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/apimodels"
)

func LogIn() string {

	for err := processEmailAddress(); err != nil; {
		fmt.Println(err.Error())
		return LogIn()
	}

	var result string
	for {
		jwtToken, err := processLoginCode()
		if err != nil {
			continue
		}
		result = jwtToken
		break
	}
	return result
}

func processEmailAddress() error {
	fmt.Print("E-Mail Address: ")

	email := readFromConsole()

	fmt.Println("Submitting to API, please wait...")

	req := apimodels.NewSubmitEmailRequest(email)
	rep := apimodels.NewSubmitEmailReply()
	err := apiClient.Post("Login/SubmitEmail", req, rep)

	if err != nil {
		return err
	}

	if !rep.Success {
		return fmt.Errorf("login failed")
	}

	fmt.Println("Please check your email for a login code.")
	return nil
}

func processLoginCode() (string, error) {
	fmt.Print("Enter login code: ")
	loginCode := readFromConsole()

	req := apimodels.NewValidateEmailRequest(loginCode)
	rep := apimodels.NewValidateEmailReply()
	err := apiClient.Post("Login/ValidateEmail", req, rep)

	if err != nil {
		return "", err
	}

	return rep.JwtToken, nil
}

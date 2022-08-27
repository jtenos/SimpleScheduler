package main

import (
	"fmt"
	"log"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
)

const API_URL = "http://localhost:5266" // TODO: Get this from config file

func main() {
	ui.Initialize(API_URL)
	ui.ShowHeader()
	valReply, err := ui.LogIn()
	if err != nil {
		log.Fatal(err)
	}
	fmt.Printf("Your JWT token is: %s", valReply.JwtToken)
}

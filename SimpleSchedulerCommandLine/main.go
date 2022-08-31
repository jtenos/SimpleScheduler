package main

import (
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
)

const API_URL = "http://localhost:5266" // TODO: Get this from config file

var jwtToken string

func main() {
	ui.Initialize(API_URL)
	ui.ShowHeader()
	jwtToken = ui.LogIn()

	ui.ShowMainMenu()
}

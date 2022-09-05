package main

import (
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/config"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
)

var conf *config.Configuration

func main() {
	conf = config.LoadConfig()

	ui.Initialize(conf.ApiUrl)
	ui.ShowHeader()
	ui.LogIn()

	ui.ShowMainMenu()
}

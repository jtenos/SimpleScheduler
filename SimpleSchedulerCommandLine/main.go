package main

import (
	"context"
	"os"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/config"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/contextkeys"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/job"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/schedule"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/user"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/worker"
)

func main() {

	cfg := config.LoadConfig()

	ctx := context.Background()
	ctx = context.WithValue(ctx, contextkeys.ApiUrlKey{}, cfg.ApiUrl)

	if len(os.Args) < 3 {
		ui.WriteHelp(1)
		return
	}

	noun := os.Args[1]
	verb := os.Args[2]

	// Remove the noun and verb, leaving only the options to be parsed later
	os.Args = os.Args[2:]

	ctx = context.WithValue(ctx, contextkeys.NounKey{}, noun)
	ctx = context.WithValue(ctx, contextkeys.VerbKey{}, verb)

	switch noun {
	case "user":
		user.Execute(ctx)
	case "job":
		job.Execute(ctx)
	case "worker":
		worker.Execute(ctx)
	case "schedule":
		schedule.Execute(ctx)
	}
}

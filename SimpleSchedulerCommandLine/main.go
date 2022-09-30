package main

import (
	"context"
	"flag"
	"fmt"
	"os"
	"sync"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/config"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/job"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/schedule"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/token"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/user"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/userdir"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/worker"
	"golang.org/x/exp/slices"
)

func main() {

	ctx := context.Background()

	if len(os.Args) < 3 {
		writeHelp(1)
		return
	}

	fmt.Println()

	hydrateContext(&ctx)

	noun := ctxhelper.GetNoun(ctx)

	switch noun {
	case "user":
		user.Execute(ctx)
	case "job":
		job.Execute(ctx)
	case "worker":
		worker.Execute(ctx)
	case "schedule":
		schedule.Execute(ctx)
	case "token":
		token.Execute(ctx)
	default:
		ui.WriteFatalf("Invalid noun: Must be 'user', 'job', 'worker', or 'schedule'")
	}
}

func hydrateContext(ctx *context.Context) {

	var wg sync.WaitGroup
	wg.Add(3)

	// API Url
	go func() {
		cfg := config.LoadConfig()
		ctxhelper.SetApiUrl(ctx, cfg.ApiUrl)
		wg.Done()
	}()

	// Noun/Verb
	go func() {
		noun := os.Args[1]
		verb := os.Args[2]

		// Remove the noun and verb, leaving only the options to be parsed later
		os.Args = os.Args[2:]

		ctxhelper.SetNoun(ctx, noun)
		ctxhelper.SetVerb(ctx, verb)

		// Not parsing here - but this will cause it to not fail in future parsing steps
		flag.Bool("verbose", false, "Display debugging information")

		verbose := slices.Contains(os.Args, "--verbose") || slices.Contains(os.Args, "-verbose")
		ctxhelper.SetVerbose(ctx, verbose)

		wg.Done()
	}()

	// Token
	go func() {
		token, err := userdir.ReadToken()
		if err != nil {
			ui.WriteFatalf("Error reading token: %s", err.Error())
		}

		ctxhelper.SetToken(ctx, token)
		wg.Done()
	}()

	wg.Wait()
}

func writeHelp(exitCode int) {
	fmt.Print(`
Usage:
  sched NOUN VERB OPTIONS
Details:

user
	login --email test@example.com
	validate --code abcdabcd-0123-4567-8910-1234567890ab

worker
	list --name "Some Work" --dir "MyDir" --exe "MyEx" --activeonly --inactiveonly
	show --id 123

schedule
    list --worker 123

job
	list --status ERR --worker 123 --workername "Some"
	run --worker 123
	details --id 123456
`)

	os.Exit(exitCode)
}

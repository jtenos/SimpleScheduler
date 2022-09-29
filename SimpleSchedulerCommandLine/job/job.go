package job

import (
	"context"
	"flag"
	"fmt"
	"unicode/utf8"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/api"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/apimodels"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
)

func Execute(ctx context.Context) {
	verb := ctxhelper.GetVerb(ctx)

	switch verb {
	case "list":
		list(ctx)
	case "run":
		run(ctx)
	case "details":
		details(ctx)
	}
}

func list(ctx context.Context) {
	//	list --status ERR --worker 123 --workername "Some"

	var status string
	var worker int64
	var workerName string

	flag.StringVar(&status, "status", "", "Job status code")
	flag.Int64Var(&worker, "worker", 0, "Worker ID")
	flag.StringVar(&workerName, "workername", "", "Search text for the worker name")
	flag.Parse()

	req := apimodels.NewGetJobsRequest(worker, workerName, status, 1, false)
	rep := apimodels.NewGetJobsReply()
	err := api.Post(ctx, "Jobs/GetJobs", req, rep)

	if err != nil {
		ui.WriteFatalf("Error retrieving workers: %s", err.Error())
	}

	fmt.Println("| Job ID    | Worker Name                                    | Start/Complete  | Status | Details |")
	for i := range rep.Jobs {
		fmt.Println("---------------------------------------------------------------------------------------------------")
		job := rep.Jobs[i]
		jobID := job.ID
		workerName := job.WorkerName
		startDate := job.QueueDateUTC
		complDate := job.CompleteDateUTC
		stat := job.StatusCode
		hasDetails := job.HasDetailedMessage

		if utf8.RuneCountInString(workerName) > 47 {
			workerName = workerName[:44] + "..."
		}

		fmt.Printf("| %-10d| %-47s| %-23s| %-20s| %-18v|\n", jobID, workerName, startDate, stat, hasDetails)
		fmt.Printf("|           |                                                | %s              |        |         |", complDate)
	}
	fmt.Println("---------------------------------------------------------------------------------------------------")

}

func run(ctx context.Context) {
	//	run --worker 123
	//
}

func details(ctx context.Context) {
	//details --id 123456

}

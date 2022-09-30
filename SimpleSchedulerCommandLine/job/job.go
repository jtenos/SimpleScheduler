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

	var workerPtr *int64
	if worker != 0 {
		workerPtr = &worker
	}

	var workerNamePtr *string
	if workerName != "" {
		workerNamePtr = &workerName
	}

	var statusPtr *string
	if status != "" {
		statusPtr = &status
	}

	req := apimodels.NewGetJobsRequest(workerPtr, workerNamePtr, statusPtr, 1, false)
	rep := apimodels.NewGetJobsReply()
	err := api.Post(ctx, "Jobs/GetJobs", req, rep)

	if err != nil {
		ui.WriteFatalf("Error retrieving workers: %s", err.Error())
	}

	fmt.Println("-------------------------------------------------------------------------------------------------------|")
	fmt.Println("| Job ID    | Worker Name                                    | Start/Complete (UTC) | Status | Details |")
	for i := range rep.Jobs {
		fmt.Println("-------------------------------------------------------------------------------------------------------|")
		job := rep.Jobs[i]
		jobID := job.ID
		workerName := job.WorkerName
		var startDate string
		if job.QueueDateUTC != nil {
			startDate = job.QueueDateUTC.Time.Format("2006-01-02 15:04:05")
		}
		var complDate string
		if job.CompleteDateUTC != nil {
			complDate = job.CompleteDateUTC.Time.Format("2006-01-02 15:04:05")
		} else {
			complDate = "                   "
		}
		stat := job.StatusCode
		hasDetails := job.HasDetailedMessage

		if utf8.RuneCountInString(workerName) > 47 {
			workerName = workerName[:44] + "..."
		}

		fmt.Printf("| %-10d| %-47s| %-21s|   %s  | %-8v|\n", jobID, workerName, startDate, stat, hasDetails)
		fmt.Printf("|           |                                                | %-21s|        |         |\n", complDate)
	}
	fmt.Println("-------------------------------------------------------------------------------------------------------|")

}

func run(ctx context.Context) {
	//	run --worker 123
	//
}

func details(ctx context.Context) {
	//details --id 123456

}

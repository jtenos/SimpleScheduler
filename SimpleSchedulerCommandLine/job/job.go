package job

import (
	"context"
	"flag"
	"fmt"
	"unicode/utf8"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/api"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxutil"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/models"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
)

func Execute(ctx context.Context) {
	verb := ctxutil.GetVerb(ctx)

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

	var status string
	var worker int64
	var workerName string
	var limit int64

	flag.StringVar(&status, "status", "", "Job status code")
	flag.Int64Var(&worker, "worker", 0, "Worker ID")
	flag.StringVar(&workerName, "workername", "", "Search text for the worker name")
	flag.Int64Var(&limit, "limit", 100, "Num results to return")
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

	type request struct {
		WorkerID    *int64  `json:"WorkerID"`
		WorkerName  *string `json:"WorkerName"`
		StatusCode  *string `json:"StatusCode"`
		RowsPerPage int32   `json:"RowsPerPage"`
		PageNumber  int32   `json:"PageNumber"`
		OverdueOnly bool    `json:"OverdueOnly"`
	}
	type reply struct {
		Jobs []models.JobWithWorkerID `json:"Jobs"`
	}

	req := request{workerPtr, workerNamePtr, statusPtr, int32(limit), 1, false}
	rep := &reply{}
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
		workerID := job.WorkerID
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
		fmt.Printf("|           | Worker ID: %-10d                          | %-21s|        |         |\n", workerID, complDate)
	}
	fmt.Println("-------------------------------------------------------------------------------------------------------|")

}

// TODO: This should live in Worker, not in Job
func run(ctx context.Context) {
	var worker int64

	flag.Int64Var(&worker, "worker", 0, "The Worker ID")
	flag.Parse()

	type request struct {
		ID int64 `json:"ID"`
	}
	type reply struct{}

	req := request{worker}
	rep := &reply{}
	err := api.Post(ctx, "Workers/RunNow", req, &rep)

	if err != nil {
		ui.WriteFatalf("Error running job: %s", err.Error())
	}

	fmt.Println("Job created")
}

func details(ctx context.Context) {
	var id int64

	flag.Int64Var(&id, "id", 0, "The Job ID")
	flag.Parse()

	type request struct {
		ID int64 `json:"ID"`
	}
	type reply struct {
		DetailedMessage *string
	}

	req := request{id}
	rep := &reply{}
	err := api.Post(ctx, "Jobs/GetDetailedMessage", req, rep)

	if err != nil {
		ui.WriteFatalf("Error retrieving workers: %s", err.Error())
	}

	if rep.DetailedMessage == nil || len(*rep.DetailedMessage) == 0 {
		fmt.Print("...No details found...")
		return
	}

	fmt.Println(*rep.DetailedMessage)
}

package worker

import (
	"context"
	"flag"
	"fmt"
	"sort"
	"strings"
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
	case "show":
		show(ctx)
	}
}

func list(ctx context.Context) {

	var name, dir, exe string
	var activeOnly, inactiveOnly bool
	flag.StringVar(&name, "name", "", "Search text for the worker name")
	flag.StringVar(&dir, "dir", "", "Search text for the directory")
	flag.StringVar(&exe, "exe", "", "Search text for the executable")
	flag.BoolVar(&activeOnly, "activeonly", false, "Only return active workers")
	flag.BoolVar(&inactiveOnly, "inactiveonly", false, "Only return inactive workers")
	flag.Parse()

	type request struct {
		WorkerName    string `json:"WorkerName"`
		DirectoryName string `json:"DirectoryName"`
		Executable    string `json:"Executable"`
		ActiveOnly    bool   `json:"ActiveOnly"`
		InactiveOnly  bool   `json:"InactiveOnly"`
	}

	type reply struct {
		Workers []models.WorkerWithSchedules `json:"workers"`
	}

	req := request{name, dir, exe, activeOnly, inactiveOnly}
	rep := &reply{}
	err := api.Post(ctx, "Workers/GetAllWorkers", req, rep)

	if err != nil {
		ui.WriteFatalf("Error retrieving workers: %s", err.Error())
	}

	for i := range rep.Workers {
		rep.Workers[i].Worker.WorkerNameLower = strings.ToLower(rep.Workers[i].Worker.WorkerName)
	}

	// Sort by worker name, but with active workers first
	sort.Slice(rep.Workers, func(i, j int) bool {
		w1 := rep.Workers[i].Worker
		w2 := rep.Workers[j].Worker

		var compare int
		if w1.IsActive && w2.IsActive {
			compare = 0
		} else if w1.IsActive {
			compare = -1
		} else if w2.IsActive {
			compare = 1
		}
		switch compare {
		case 0:
			return w1.WorkerNameLower < w2.WorkerNameLower
		case 1:
			return false
		case -1:
			return true
		}
		return false
	})

	fmt.Println("| ID        | Name                                           | Directory              | Executable          | Arguments         |")
	for i := range rep.Workers {
		fmt.Println("---------------------------------------------------------------------------------------------------------------------------------")
		worker := rep.Workers[i].Worker
		workerID := rep.Workers[i].Worker.ID
		title := worker.WorkerName
		dir := worker.DirectoryName
		exe := worker.Executable
		args := worker.ArgumentValues

		if worker.IsActive {
			if utf8.RuneCountInString(title) > 47 {
				title = title[:44] + "..."
			}
		} else {
			if utf8.RuneCountInString(title) > 36 {
				title = title[:33] + "..."
			}
			title += " (INACTIVE)"
		}

		if utf8.RuneCountInString(dir) > 23 {
			dir = dir[:20] + "..."
		}

		if utf8.RuneCountInString(exe) > 20 {
			exe = exe[:17] + "..."
		}

		if utf8.RuneCountInString(args) > 18 {
			args = args[:15] + "..."
		}

		fmt.Printf("| %-10d| %-47s| %-23s| %-20s| %-18s|\n", workerID, title, dir, exe, args)
	}
	fmt.Println("---------------------------------------------------------------------------------------------------------------------------------")

}

func show(ctx context.Context) {
	//show --id 123

	// type request struct {
	// 	ID int64 `json:"id"`
	// }
	// type reply struct {
	// 	Worker *models.WorkerWithSchedules `json:"worker"`
	// }

	// func getWorker(workerID int64) (*models.WorkerWithSchedules, error) {
	// req := request{workerID}
	// 	// rep := &reply{}
	// 	// err := api.Post("Workers/GetWorker", apimodels.NewGetWorkerRequest(workerID), rep)
	// 	// if err != nil {
	// 	// return nil, err
	// 	// }
	// 	// return rep.Worker, nil

	// 	return nil, nil
	// }

}

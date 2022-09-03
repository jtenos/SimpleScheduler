package ui

import (
	"fmt"
	"sort"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/apimodels"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/models"
)

func showWorkers() {
	newMenu("WORKERS MENU",
		[]*menuItem{
			newMenuItem("List Workers", listWorkers),
			newMenuItem("Search Workers", searchWorkers),
		},
		ShowMainMenu,
	).show()
}

func listWorkers() {
	req := apimodels.NewGetAllWorkersRequest()
	rep := apimodels.NewGetAllWorkersReply()
	err := apiClient.Post("Workers/GetAllWorkers", req, rep)

	if err != nil {
		fmt.Printf("%v\n", err)
		return
	}

	// Sort by worker name
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
			return w1.WorkerName < w2.WorkerName
		case 1:
			return false
		case -1:
			return true
		}
		return false
	})

	menuItems := make([]*menuItem, len(rep.Workers))
	for i := range rep.Workers {
		wws := &rep.Workers[i]
		worker := &rep.Workers[i].Worker
		title := worker.WorkerName
		if !worker.IsActive {
			title += " (INACTIVE)"
		}
		menuItems[i] = newMenuItem(title, func() { showWorker(wws, rep.Workers) })
	}

	newMenu("WORKERS",
		menuItems,
		showWorkers,
	).show()
}

func getWorkerName(workerID int64, allWorkers []models.WorkerWithSchedules) string {
	for i := range allWorkers {
		if allWorkers[i].Worker.ID == workerID {
			return allWorkers[i].Worker.WorkerName
		}
	}
	return "NOT FOUND"
}

func showWorker(worker *models.WorkerWithSchedules, allWorkers []models.WorkerWithSchedules) {
	fmt.Println("")
	fmt.Printf("Worker: %s", worker.Worker.WorkerName)
	if worker.Worker.IsActive {
		fmt.Printf(" (active)\n")
	} else {
		fmt.Printf(" (INACTIVE)\n")
	}
	if len(worker.Worker.DetailedDescription) > 0 {
		fmt.Printf("  Description: %s\n", worker.Worker.DetailedDescription)
	}
	if len(worker.Worker.EmailOnSuccess) > 0 {
		fmt.Printf("  Email on Success: %s\n", worker.Worker.EmailOnSuccess)
	}
	if worker.Worker.ParentWorkerID > 0 {
		fmt.Printf("  Parent worker: %s\n", getWorkerName(worker.Worker.ParentWorkerID, allWorkers))
	}
	fmt.Printf("  Timeout minutes: %v\n", worker.Worker.TimeoutMinutes)
	fmt.Printf("  Directory: %s\n", worker.Worker.DirectoryName)
	fmt.Printf("  Executable: %s\n", worker.Worker.Executable)
	if len(worker.Worker.ArgumentValues) > 0 {
		fmt.Printf("Arguments: %s\n", worker.Worker.ArgumentValues)
	}
	fmt.Println("  Schedules:")
	for _, sch := range worker.Schedules {
		fmt.Printf("    %s\n", sch.GetFormatted())
	}
}

func searchWorkers() {
	fmt.Println("TODO: SEARCH WORKERS")
	showWorkers()
}

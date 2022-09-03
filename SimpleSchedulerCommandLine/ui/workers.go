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

	sort.Slice(rep.Workers, func(i, j int) bool {
		return rep.Workers[i].Worker.WorkerName < rep.Workers[j].Worker.WorkerName
	})
	menuItems := make([]*menuItem, len(rep.Workers))
	for i := range rep.Workers {
		worker := &rep.Workers[i].Worker
		menuItems[i] = newMenuItem(rep.Workers[i].Worker.WorkerName, func() { showWorker(worker) })
	}
	newMenu("WORKERS",
		menuItems,
		showWorkers,
	).show()
}

func showWorker(worker *models.Worker) {
	fmt.Printf("Worker: %s\n", worker.WorkerName)
	// TODO: Details
}

func searchWorkers() {
	fmt.Println("TODO: SEARCH WORKERS")
	showWorkers()
}

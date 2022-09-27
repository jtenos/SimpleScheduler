package ui

import (
	"fmt"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/models"
)

func showWorkers() {
	newMenu("WORKERS",
		[]*menuItem{
			newMenuItem("List Workers", listWorkers),
			newMenuItem("Search Workers", searchWorkers),
		},
		ShowMainMenu,
	).show()
}

func listWorkers() {
	// req := apimodels.NewGetAllWorkersRequest()
	// rep := apimodels.NewGetAllWorkersReply()
	// err := api.Post("Workers/GetAllWorkers", req, rep)

	// if err != nil {
	// 	fmt.Printf("%v\n", err)
	// 	return
	// }

	// // Sort by worker name
	// sort.Slice(rep.Workers, func(i, j int) bool {
	// 	w1 := rep.Workers[i].Worker
	// 	w2 := rep.Workers[j].Worker

	// 	var compare int
	// 	if w1.IsActive && w2.IsActive {
	// 		compare = 0
	// 	} else if w1.IsActive {
	// 		compare = -1
	// 	} else if w2.IsActive {
	// 		compare = 1
	// 	}
	// 	switch compare {
	// 	case 0:
	// 		return w1.WorkerName < w2.WorkerName
	// 	case 1:
	// 		return false
	// 	case -1:
	// 		return true
	// 	}
	// 	return false
	// })

	// menuItems := make([]*menuItem, len(rep.Workers))
	// for i := range rep.Workers {
	// 	worker := &rep.Workers[i].Worker
	// 	workerID := &rep.Workers[i].Worker.ID
	// 	title := worker.WorkerName
	// 	if !worker.IsActive {
	// 		title += " (INACTIVE)"
	// 	}
	// 	menuItems[i] = newMenuItem(title, func() { showWorker(*workerID, rep.Workers) })
	// }

	// newMenu("WORKERS",
	// 	menuItems,
	// 	showWorkers,
	// ).show()
}

func getWorkerName(workerID int64, allWorkers []models.WorkerWithSchedules) string {
	for i := range allWorkers {
		if allWorkers[i].Worker.ID == workerID {
			return allWorkers[i].Worker.WorkerName
		}
	}
	return "NOT FOUND"
}

func showWorker(workerID int64, allWorkers []models.WorkerWithSchedules) {
	worker, err := getWorker(workerID)
	if err != nil {
		writeError(err.Error())
		return
	}
	displayWorkerDetails(worker, allWorkers)

	newMenu("WORKER",
		[]*menuItem{
			newMenuItem("Edit Worker", func() { editWorker(workerID, allWorkers) }),
			newMenuItem("Edit Schedules", func() { editSchedules(worker) }),
		}, showWorkers,
	).show()
}

func displayWorkerDetails(worker *models.WorkerWithSchedules, allWorkers []models.WorkerWithSchedules) {
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
	if worker.Worker.ParentWorkerID != nil && *worker.Worker.ParentWorkerID > 0 {
		fmt.Printf("  Parent worker: %s\n", getWorkerName(*worker.Worker.ParentWorkerID, allWorkers))
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

func editWorker(workerID int64, allWorkers []models.WorkerWithSchedules) {

	worker, err := getWorker(workerID)
	if err != nil {
		writeError(err.Error())
		editWorker(workerID, allWorkers)
		return
	}

	makeActive := func() {
		defer editWorker(workerID, allWorkers)
		fmt.Println("makeActive")
	}
	makeInactive := func() {
		defer editWorker(workerID, allWorkers)
		fmt.Println("makeInactive")
	}
	editDescription := func() {
		defer editWorker(workerID, allWorkers)
		fmt.Println("editDescription")
	}
	editEmail := func() {
		defer editWorker(workerID, allWorkers)
		fmt.Println("editEmail")
	}
	editTimeout := func() {
		defer editWorker(workerID, allWorkers)
		fmt.Println("editTimeout")
	}
	editExecutable := func() {
		defer editWorker(workerID, allWorkers)
		fmt.Println("editDirectory")
		fmt.Println("editExecutable")
	}
	editArguments := func() {
		defer editWorker(workerID, allWorkers)
		fmt.Println("editArguments")
	}

	var activeInactiveLabel string
	var activeInactiveFunc func()
	if worker.Worker.IsActive {
		activeInactiveLabel = "Deactivate Worker"
		activeInactiveFunc = makeInactive
	} else {
		activeInactiveLabel = "Activate Worker"
		activeInactiveFunc = makeActive
	}

	displayWorkerDetails(worker, allWorkers)

	newMenu("WORKER",
		[]*menuItem{
			newMenuItem("Edit Name", func() { editName(worker.Worker.ID, allWorkers) }),
			newMenuItem(activeInactiveLabel, activeInactiveFunc),
			newMenuItem("Edit Description", editDescription),
			newMenuItem("Edit Email on Success", editEmail),
			newMenuItem("Edit Parent Worker", func() { editParent(worker.Worker.ID, allWorkers) }),
			newMenuItem("Edit Timeout", editTimeout),
			newMenuItem("Edit Directory/Executable", editExecutable),
			newMenuItem("Edit Arguments", editArguments),
		}, showWorkers,
	).show()
}

func editName(workerID int64, allWorkers []models.WorkerWithSchedules) {
	// defer editWorker(workerID, allWorkers)

	// worker, err := getWorker(workerID)
	// if err != nil {
	// 	writeError(err.Error())
	// 	return
	// }

	// fmt.Printf("Old name: %s\n", worker.Worker.WorkerName)
	// fmt.Print("New name: ")
	// newName := readFromConsole()
	// if len(newName) == 0 || len(newName) > models.WORKER_NAME_MAX_LENGTH {
	// 	writeError(fmt.Sprintf("Invalid name - must be between 1 and %v characters", models.WORKER_NAME_MAX_LENGTH))
	// 	return
	// }

	// postObj := apimodels.NewUpdateWorkerRequest(worker.Worker.ID, newName, worker.Worker.DetailedDescription,
	// 	worker.Worker.EmailOnSuccess, worker.Worker.ParentWorkerID, worker.Worker.TimeoutMinutes,
	// 	worker.Worker.DirectoryName, worker.Worker.Executable, worker.Worker.ArgumentValues)

	// resultObj := apimodels.NewUpdateWorkerReply()

	// err = api.Post("Workers/UpdateWorker", postObj, resultObj)

	// if err != nil {
	// 	writeError(fmt.Sprintf("Error: %v", err))
	// 	return
	// }
}

func editParent(workerID int64, allWorkers []models.WorkerWithSchedules) {

	// wws, err := getWorker(workerID)
	// if err != nil {
	// 	writeError(err.Error())
	// 	return
	// }

	// req := apimodels.NewGetAllWorkersRequest()
	// rep := apimodels.NewGetAllWorkersReply()
	// err = api.Post("Workers/GetAllWorkers", req, rep)

	// if err != nil {
	// 	writeError(err.Error())
	// 	return
	// }

	// workers := make([]models.Worker, 0)
	// for _, w := range rep.Workers {
	// 	if w.Worker.IsActive {
	// 		workers = append(workers, w.Worker)
	// 	}
	// }

	// // Sort by worker name
	// sort.Slice(workers, func(i, j int) bool {
	// 	w1 := workers[i]
	// 	w2 := workers[j]

	// 	return w1.WorkerName < w2.WorkerName
	// })

	// menuItems := make([]*menuItem, len(workers))
	// for i := range workers {
	// 	worker := &workers[i]
	// 	title := worker.WorkerName
	// 	menuItems[i] = newMenuItem(title, func() {
	// 		defer editWorker(wws.Worker.ID, allWorkers)

	// 		postObj := apimodels.NewUpdateWorkerRequest(wws.Worker.ID, wws.Worker.WorkerName, wws.Worker.DetailedDescription,
	// 			wws.Worker.EmailOnSuccess, &worker.ID, wws.Worker.TimeoutMinutes,
	// 			wws.Worker.DirectoryName, wws.Worker.Executable, wws.Worker.ArgumentValues)

	// 		resultObj := apimodels.NewUpdateWorkerReply()

	// 		err := api.Post("Workers/UpdateWorker", postObj, resultObj)

	// 		if err != nil {
	// 			writeError(fmt.Sprintf("Error: %v", err))
	// 			return
	// 		}
	// 	})
	// }

	// // TODO: Add a "no parent" option
	// newMenu("CHOOSE NEW PARENT",
	// 	menuItems,
	// 	func() { editWorker(wws.Worker.ID, allWorkers) },
	// ).show()
}

func editSchedules(worker *models.WorkerWithSchedules) {
	addSchedule := func() {

	}
	editSchedule := func() {

	}
	newMenu("SCHEDULES",
		[]*menuItem{
			newMenuItem("Add Schedule", addSchedule),
			newMenuItem("Edit Schedule", editSchedule),
		},
		showWorkers,
	).show()
}

func getWorker(workerID int64) (*models.WorkerWithSchedules, error) {
	// rep := apimodels.NewGetWorkerReply()
	// err := api.Post("Workers/GetWorker", apimodels.NewGetWorkerRequest(workerID), rep)
	// if err != nil {
	// return nil, err
	// }
	// return rep.Worker, nil

	return nil, nil
}

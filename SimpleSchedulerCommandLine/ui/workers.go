package ui

import "fmt"

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
	fmt.Println("TODO: LIST WORKERS")
	showWorkers()
}

func searchWorkers() {
	fmt.Println("TODO: SEARCH WORKERS")
	showWorkers()
}

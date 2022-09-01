package ui

import (
	"os"
)

func ShowMainMenu() {
	newMenu(
		"MAIN MENU",
		[]*menuItem{
			newMenuItem("Jobs", showJobs),
			newMenuItem("Workers", showWorkers),
		}, func() {
			os.Exit(0)
		},
	).show()
}

package ui

import (
	"fmt"
	"os"
	"strings"
)

func ShowMainMenu() {
	for {
		fmt.Print("\n*** MAIN MENU ***\n\n")
		fmt.Println("1: Jobs")
		fmt.Println("2: Workers")
		fmt.Println("")
		fmt.Print("Make a selection (EXIT to exit): ")
		sel := readFromConsole()
		switch strings.ToLower(sel) {
		case "1":
			showJobs()
		case "2":
			showWorkers()
		case "exit":
			os.Exit(0)
		}
	}
}

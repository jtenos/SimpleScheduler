package ui

import (
	"bufio"
	"fmt"
	"os"
	"strings"
)

const COLOR_RESET = "\033[0m"
const COLOR_RED = "\033[31m"

var rdr = bufio.NewReader(os.Stdin)

func ShowHeader() {
	fmt.Print("\n*** SIMPLE SCHEDULER ***\n\n")
}

func readFromConsole() string {
	text, _ := rdr.ReadString('\n')
	text = strings.TrimSpace(text)
	return text
}

func writeError(msg string) {
	fmt.Printf("%s %s %s", COLOR_RED, msg, COLOR_RESET)
}

func WriteHelp(exitCode int) {
	fmt.Println("Usage:")
	fmt.Println("  Sched NOUN VERB OPTIONS")
	fmt.Println("Details:")
	fmt.Println("  user")
	fmt.Println("    login")
	fmt.Println("      --email test@example.com")
	fmt.Println("    validate")
	fmt.Println("      --code abcdabcd-0123-4567-8910-1234567890ab")
	fmt.Println("  job")
	fmt.Println("    list")
	fmt.Println("      --status ERR")
	fmt.Println("      --name \"Some*\"")
	fmt.Println("  worker")
	// TODO: Complete this
	fmt.Println("  schedule")
	os.Exit(exitCode)
}

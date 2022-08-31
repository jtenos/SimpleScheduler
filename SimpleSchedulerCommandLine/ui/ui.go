package ui

import (
	"bufio"
	"fmt"
	"os"
	"strings"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/api"
)

var rdr = bufio.NewReader(os.Stdin)

var apiClient *api.ApiClient

func Initialize(baseUrl string) {
	apiClient = api.NewApiClient(baseUrl)
}

func ShowHeader() {
	fmt.Print("\n*** SIMPLE SCHEDULER ***\n\n")
}

func readFromConsole() string {
	text, _ := rdr.ReadString('\n')
	text = strings.TrimSpace(text)
	return text
}

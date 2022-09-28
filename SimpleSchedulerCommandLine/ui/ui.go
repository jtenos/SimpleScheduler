package ui

import (
	"fmt"
	"log"
)

const COLOR_RESET = "\033[0m"
const COLOR_RED = "\033[31m"

func WriteFatalf(format string, a ...any) {
	msg := fmt.Sprintf(format, a...)
	log.Fatalf("%s %s %s", COLOR_RED, msg, COLOR_RESET)
}

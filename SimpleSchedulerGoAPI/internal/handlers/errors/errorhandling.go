package errors

import (
	"fmt"
	"log"
	"net/http"
)

func HandleError(w http.ResponseWriter, r *http.Request, statusCode int, errMsg string, showError bool) {
	w.Header().Set("Content-Type", "text/plain")
	w.WriteHeader(statusCode)
	if showError {
		fmt.Fprintf(w, "%d %s\n", statusCode, errMsg)
	} else {
		fmt.Fprintf(w, "%d An error has occurred.\n", statusCode)
	}
	log.Printf("Status %d: %s", statusCode, errMsg)
}

package errorhandling

import (
	"fmt"
	"log"
	"net/http"
)

func HandleError(w http.ResponseWriter, r *http.Request, err error, caller string, statusCode int) {
	w.Header().Set("Content-Type", "text/plain")

	fmt.Fprintf(w, "%d An error has occurred.\n%s", statusCode, err.Error())
	w.WriteHeader(statusCode)

	log.Printf("Status %d: %s | %s", statusCode, err.Error(), caller)
}

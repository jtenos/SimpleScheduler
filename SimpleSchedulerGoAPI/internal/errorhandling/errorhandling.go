package errorhandling

import (
	"errors"
	"fmt"
	"log"
	"net/http"
)

func HandleError(w http.ResponseWriter, r *http.Request, err error) {
	w.Header().Set("Content-Type", "text/plain")

	var statusCode int
	var fe FriendlyError
	if errors.As(err, &fe) {
		statusCode = fe.StatusCode()
		fmt.Fprintf(w, "%d %s\n", statusCode, err.Error())
		w.WriteHeader(statusCode)
	} else {
		statusCode = http.StatusInternalServerError
		fmt.Fprintf(w, "%d An error has occurred.\n", statusCode)
		w.WriteHeader(statusCode)
	}

	log.Printf("Status %d: %s", statusCode, err.Error())
}

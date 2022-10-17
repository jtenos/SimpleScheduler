package errors

import (
	"log"
	"net/http"
)

func HandleError(w http.ResponseWriter, r *http.Request, statusCode int, errMsg string) {
	w.WriteHeader(statusCode)
	w.Write([]byte("An error has occurred."))
	log.Printf("Status %d: %s", statusCode, errMsg)
}
